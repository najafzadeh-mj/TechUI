(function () {

    const doc = document.documentElement;
    const toggle = document.getElementById('themeToggle');
    const toggleText = document.getElementById('themeToggleText');
    const preset = document.getElementById('presetPicker');
    const sidebar = document.getElementById('adminSidebar');
    const sidebarToggle = document.getElementById('sidebarToggle');

    function syncChrome() {

        const theme = Tech.theme.get();

        doc.setAttribute('data-bs-theme', theme);

        if (preset) preset.value = Tech.theme.getPreset();

        if (toggleText) {
            toggleText.textContent = theme === 'dark' ? 'تم روشن' : 'تم تاریک';
        }

        if (toggle) {
            const icon = toggle.querySelector('i');

            if (icon) {
                icon.className = theme === 'dark'
                    ? 'bi bi-sun'
                    : 'bi bi-moon-stars';
            }
        }
    }

    syncChrome();

    document.addEventListener('tech:theme-changed', syncChrome);

    if (toggle) {
        toggle.addEventListener('click', function () {
            Tech.theme.toggle();
        });
    }

    if (preset) {
        preset.addEventListener('change', function () {
            Tech.theme.setPreset(preset.value);
        });
    }

    if (sidebar && sidebarToggle) {

        sidebarToggle.addEventListener('click', function (ev) {
            ev.stopPropagation();
            sidebar.classList.toggle('is-open');
        });

        document.addEventListener('click', function (ev) {

            if (!sidebar.classList.contains('is-open')) return;

            if (sidebar.contains(ev.target) ||
                sidebarToggle.contains(ev.target)) {
                return;
            }

            sidebar.classList.remove('is-open');
        });
    }

    const toastData = document.getElementById('admin-toast-data');

    if (toastData) {

        if (toastData.dataset.success) {
            Tech.notify(toastData.dataset.success, {
                type: 'success',
                title: 'انجام شد'
            });
        }

        if (toastData.dataset.error) {
            Tech.notify(toastData.dataset.error, {
                type: 'danger',
                title: 'خطا'
            });
        }
    }

    function antiforgeryToken() {

        const meta = document.querySelector(
            'meta[name="__RequestVerificationToken"]');

        if (meta) return meta.getAttribute('content');

        const input = document.querySelector(
            'input[name="__RequestVerificationToken"]');

        return input ? input.value : null;
    }

    async function adminPost(url, data) {

        const headers = {
            'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8',
            'Accept': 'application/json'
        };

        const token = antiforgeryToken();

        if (token) headers['RequestVerificationToken'] = token;

        const response = await fetch(url, {
            method: 'POST',
            headers: headers,
            body: new URLSearchParams(data || {}).toString(),
            credentials: 'same-origin'
        });

        if (!response.ok) {
            throw new Error('HTTP ' + response.status);
        }

        return response.json();
    }

    async function confirmDelete(url, id, message) {

        const confirmed = await Tech.confirm(message, {
            title: 'تأیید حذف',
            type: 'warning',
            confirmText: 'حذف',
            confirmVariant: 'danger'
        });

        if (!confirmed) return;

        try {

            const result = await adminPost(url, { id: id });

            Tech.notify(result.message || 'عملیات انجام شد.', {
                type: result.success ? 'success' : 'danger',
                title: result.success ? 'انجام شد' : 'امکان انجام عملیات'
            });

            if (result.success) {
                setTimeout(function () {
                    window.location.reload();
                }, 700);
            }
        }
        catch (err) {
            console.error(err);

            Tech.notify('ارتباط با سرور برقرار نشد.', {
                type: 'danger',
                title: 'خطا'
            });
        }
    }

    window.deleteCustomer = function (id) {
        return confirmDelete(
            '/Admin/CustomerDelete',
            id,
            'آیا از حذف این مشتری مطمئن هستید؟');
    };

    window.deleteProduct = function (id) {
        return confirmDelete(
            '/Admin/ProductDelete',
            id,
            'آیا از حذف این محصول مطمئن هستید؟');
    };

    window.deleteOrder = function (id) {
        return confirmDelete(
            '/Admin/OrderDelete',
            id,
            'آیا از حذف این سفارش مطمئن هستید؟');
    };
})();
