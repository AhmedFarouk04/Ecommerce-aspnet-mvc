window.toggleWishlist = function (productId, btn) {

    const isActive = btn.classList.contains("active");
    const url = isActive ? "/Wishlist/Remove" : "/Wishlist/Add";

    $.post(url, { productId }, function (res) {

        if (!res.success) {
            if (window.showToast) showToast("You must login first.", "warning");
            return;
        }

        btn.classList.toggle("active");
        btn.innerHTML = btn.classList.contains("active")
            ? `<i class="bi bi-heart-fill text-danger fs-4"></i>`
            : `<i class="bi bi-heart fs-4"></i>`;

        const counter = document.getElementById("wishlistCounter");
        if (counter) counter.textContent = res.count;

        
        if (window.showToast)
            showToast(isActive ? "Removed from wishlist" : "Added to wishlist", "success");
    });
};
