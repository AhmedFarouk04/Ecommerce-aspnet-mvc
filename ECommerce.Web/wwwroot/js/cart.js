document.addEventListener("DOMContentLoaded", function () {

    const cartToggle = document.getElementById("cartDropdown");
    const cartMenu = document.getElementById("mini-cart-dropdown");

   
    if (cartToggle && cartMenu) {

        cartToggle.addEventListener("click", function (e) {
            e.preventDefault();
            e.stopPropagation();

            if (cartMenu.style.display === "block") {
                cartMenu.style.display = "none";
                return;
            }

            refreshCartState(true);
        });

        document.addEventListener("click", function () {
            cartMenu.style.display = "none";
        });

        cartMenu.addEventListener("click", function (e) {
            e.stopPropagation();
        });
    }

   
    function updateCartCounter(count) {
        const badge = document.getElementById("cartCounter");
        if (!badge) return;

        badge.textContent = count;
        badge.style.display = count > 0 ? "block" : "none";
    }

    
    function updateProductStock(productId, availableStock) {
        const card = document.querySelector(`[data-product-id="${productId}"]`);
        if (!card) return;

        const badge = card.querySelector(".product-stock-badge");
        const addBtn = card.querySelector(".add-to-cart-btn");

        if (!badge || !addBtn) return;

        if (availableStock > 10) {
            badge.className = "badge bg-success mb-2 align-self-start product-stock-badge";
            badge.textContent = "In Stock";
            addBtn.disabled = false;
        }
        else if (availableStock > 0) {
            badge.className = "badge bg-warning text-dark mb-2 align-self-start product-stock-badge";
            badge.textContent = `Only ${availableStock} left`;
            addBtn.disabled = false;
        }
        else {
            badge.className = "badge bg-danger mb-2 align-self-start product-stock-badge";
            badge.textContent = "Out of Stock";
            addBtn.disabled = true;
        }
    }

    
    function refreshCartState(openDropdown = false) {
        fetch("/Cart/State")
            .then(r => r.json())
            .then(state => {
                updateCartCounter(state.count);
                renderMiniCart(state);

                if (openDropdown && cartMenu) {
                    cartMenu.style.display = "block";
                }
            });
    }


    function renderMiniCart(state) {
        if (!cartMenu) return;

        if (!state || state.count === 0) {
            cartMenu.innerHTML = `
                <div class="p-4 text-center">
                    <i class="bi bi-cart-x fs-3 text-muted"></i>
                    <p class="mt-2 mb-2">Your cart is empty.</p>
                    <a href="/Products" class="btn btn-outline-primary btn-sm">
                        Start Shopping
                    </a>
                </div>`;
            return;
        }

        let itemsHtml = "";

        state.items.forEach(item => {
            itemsHtml += `
                <div class="d-flex align-items-center gap-2 p-2 border-bottom">
                    <img src="/images/products/${item.image || "no-image.png"}"
                         class="rounded"
                         style="width:50px;height:50px;object-fit:cover" />

                    <div class="flex-grow-1">
                        <div class="fw-semibold">${item.name}</div>
                        <small class="text-muted">
                            Qty: ${item.quantity} × $${item.price}
                        </small>
                    </div>

                    <button class="btn btn-sm btn-link text-danger"
                            onclick="removeFromCartAjax(${item.productId})">
                        <i class="bi bi-x"></i>
                    </button>
                </div>`;
        });

        cartMenu.innerHTML = `
            <div>
                ${itemsHtml}
                <div class="p-3">
                    <div class="d-flex justify-content-between fw-bold mb-2">
                        <span>Total</span>
                        <span class="text-success">$${state.total.toFixed(2)}</span>
                    </div>
                    <a href="/Cart" class="btn btn-outline-dark w-100 mb-2">
                        View Cart
                    </a>
                    <a href="/Checkout" class="btn btn-success w-100">
                        Checkout
                    </a>
                </div>
            </div>`;
    }

  
    window.addToCartAjax = function (productId) {
        fetch("/Cart/AddAjax", {
            method: "POST",
            headers: { "Content-Type": "application/x-www-form-urlencoded; charset=UTF-8" },
            body: "id=" + encodeURIComponent(productId)
        })
            .then(r => r.json())
            .then(res => {
                if (!res.success) {
                    showToast?.(res.message, "danger");
                    return;
                }

                showToast?.(res.message, "success");

                refreshCartState();

                document.dispatchEvent(new CustomEvent("cart:updated", {
                    detail: {
                        productId: res.productId,
                        availableStock: res.availableStock
                    }
                }));

                if (res.productId && res.availableStock !== undefined) {
                    updateProductStock(res.productId, res.availableStock);
                }
            });
    };

 
    window.updateQty = function (productId, delta) {
        fetch("/Cart/UpdateQty", {
            method: "POST",
            headers: { "Content-Type": "application/x-www-form-urlencoded; charset=UTF-8" },
            body: `productId=${productId}&delta=${delta}`
        })
            .then(r => r.json())
            .then(res => {

                if (!res.success) {
                    showToast?.(res.message || "Stock limit reached", "warning");
                    return;
                }

                const qtyInput = document.getElementById(`qty-${productId}`);
                const itemTotal = document.getElementById(`item-total-${productId}`);

                if (qtyInput) qtyInput.value = res.quantity;
                if (itemTotal) itemTotal.innerText = `$${res.itemTotal.toFixed(2)}`;

                const countEl = document.getElementById("orderItemsCount");
                const totalEl = document.getElementById("orderTotal");

                if (countEl) countEl.innerText = res.count;
                if (totalEl) totalEl.innerText = `$${res.total.toFixed(2)}`;

                refreshCartState();

                document.dispatchEvent(new CustomEvent("cart:updated", {
                    detail: {
                        productId: productId,
                        availableStock: res.availableStock
                    }
                }));
            });
    };

   
    window.removeFromCartAjax = function (productId) {
        fetch("/Cart/RemoveAjax", {
            method: "POST",
            headers: { "Content-Type": "application/x-www-form-urlencoded; charset=UTF-8" },
            body: "id=" + encodeURIComponent(productId)
        })
            .then(r => r.json())
            .then(res => {

                const row = document.getElementById(`cart-row-${productId}`);
                if (row) row.remove();

                const countEl = document.getElementById("orderItemsCount");
                const totalEl = document.getElementById("orderTotal");

                if (countEl) countEl.innerText = res.count;
                if (totalEl) totalEl.innerText = `$${res.total.toFixed(2)}`;

                refreshCartState();

                document.dispatchEvent(new CustomEvent("cart:updated", {
                    detail: {
                        productId: productId,
                        availableStock: res.availableStock
                    }
                }));
            });
    };

   
    refreshCartState();
});
