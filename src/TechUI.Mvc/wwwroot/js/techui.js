window.Tech = window.Tech || {};

// ============================================================
// Shared helpers
// ============================================================

Tech.escapeHtml = function (value) {

    if (value === null || value === undefined) return '';

    return String(value).replace(/[&<>"']/g, function (c) {

        return {
            '&': '&amp;',
            '<': '&lt;',
            '>': '&gt;',
            '"': '&quot;',
            "'": '&#39;'
        }[c];
    });
};

Tech.getAntiforgeryToken = function (element) {

    if (element && element.dataset && element.dataset.antiforgery) {
        return element.dataset.antiforgery;
    }

    const input = document.querySelector(
        'input[name="__RequestVerificationToken"]');

    if (input && input.value) return input.value;

    const meta = document.querySelector(
        'meta[name="__RequestVerificationToken"]');

    return meta ? meta.getAttribute('content') : null;
};

Tech.requestJson = async function (url, options) {

    options = options || {};

    if (!url) throw new Error('TechUI: data source URL is missing.');

    const method = (options.method || 'GET').toUpperCase();

    const params = options.params instanceof URLSearchParams
        ? options.params
        : new URLSearchParams(options.params || {});

    const headers = Object.assign(
        { 'Accept': 'application/json' },
        options.headers || {});

    const init = {
        method: method,
        headers: headers,
        credentials: 'same-origin'
    };

    let target = url;

    if (method === 'GET') {

        const query = params.toString();

        if (query) {
            target += (url.indexOf('?') >= 0 ? '&' : '?') + query;
        }
    }
    else {

        init.body = params.toString();

        headers['Content-Type'] =
            'application/x-www-form-urlencoded; charset=UTF-8';

        const token = Tech.getAntiforgeryToken(options.antiforgeryElement);

        if (token) {
            headers['RequestVerificationToken'] = token;
        }
    }

    const response = await fetch(target, init);

    if (!response.ok) {
        throw new Error(
            `HTTP ${response.status} ${response.statusText}`.trim());
    }

    const contentType = response.headers.get('content-type') || '';

    if (contentType.indexOf('application/json') >= 0) {
        return response.json();
    }

    const text = await response.text();

    try {
        return JSON.parse(text);
    }
    catch {
        throw new Error('TechUI: response is not valid JSON.');
    }
};

Tech.trigger = function (element, name, payload) {

    if (!element || !element.dataset) return;

    const handlerName = element.dataset['event' + name];

    if (!handlerName) return;

    const fn = window[handlerName];

    if (typeof fn === 'function') {
        fn(payload);
    }
};

// ============================================================
// Theme
// ============================================================

Tech.theme = {

    storageKey: 'tech-theme',
    presetStorageKey: 'tech-theme-preset',
    presets: ['default', 'emerald', 'violet', 'rose', 'amber', 'slate'],

    get: function () {

        return document.documentElement
            .getAttribute('data-tech-theme') === 'dark'
            ? 'dark'
            : 'light';
    },

    getPreset: function () {

        const preset = document.documentElement
            .getAttribute('data-tech-preset') || 'default';

        return Tech.theme.presets.indexOf(preset) >= 0
            ? preset
            : 'default';
    },

    notify: function () {

        document.dispatchEvent(new CustomEvent(
            'tech:theme-changed', {
                detail: {
                    theme: Tech.theme.get(),
                    preset: Tech.theme.getPreset()
                }
            }));
    },

    set: function (theme, persist) {

        theme = theme === 'dark' ? 'dark' : 'light';

        document.documentElement.setAttribute('data-tech-theme', theme);

        if (persist !== false) {
            try {
                localStorage.setItem(Tech.theme.storageKey, theme);
            }
            catch {
                // Storage may be unavailable (private mode).
            }
        }

        Tech.theme.notify();

        return theme;
    },

    setPreset: function (preset, persist) {

        preset = Tech.theme.presets.indexOf(preset) >= 0
            ? preset
            : 'default';

        document.documentElement.setAttribute('data-tech-preset', preset);

        if (persist !== false) {
            try {
                localStorage.setItem(
                    Tech.theme.presetStorageKey, preset);
            }
            catch {
                // Storage may be unavailable (private mode).
            }
        }

        Tech.theme.notify();

        return preset;
    },

    toggle: function () {

        return Tech.theme.set(
            Tech.theme.get() === 'dark' ? 'light' : 'dark');
    }
};

try {
    const savedTheme = localStorage.getItem(Tech.theme.storageKey);

    if (savedTheme === 'dark' || savedTheme === 'light') {
        document.documentElement.setAttribute('data-tech-theme', savedTheme);
    }

    const savedPreset = localStorage.getItem(Tech.theme.presetStorageKey);

    if (savedPreset && Tech.theme.presets.indexOf(savedPreset) >= 0) {
        document.documentElement
            .setAttribute('data-tech-preset', savedPreset);
    }
}
catch {
    // Ignore storage access errors.
}

// ============================================================
// Template engine
// ============================================================

Tech.TemplateHelpers = Tech.TemplateHelpers || {

    formatDate(value, locale = 'fa-IR') {

        if (!value) return '';

        try {
            return new Intl.DateTimeFormat(locale).format(new Date(value));
        }
        catch {
            return value;
        }
    },

    formatDateTime(value, locale = 'fa-IR') {

        if (!value) return '';

        try {
            return new Intl.DateTimeFormat(locale, {
                dateStyle: 'short',
                timeStyle: 'short'
            }).format(new Date(value));
        }
        catch {
            return value;
        }
    },

    formatNumber(value, decimals = 0) {

        if (value === null || value === undefined || value === '') return '';

        return Number(value).toLocaleString(undefined, {
            minimumFractionDigits: decimals,
            maximumFractionDigits: decimals
        });
    },

    badge(text, color = 'secondary') {

        return `<span class="badge bg-${Tech.escapeHtml(color)}">` +
            `${Tech.escapeHtml(text)}</span>`;
    },

    yesNo(value) {
        return value ? 'بله' : 'خیر';
    },

    boolBadge(value) {

        return value
            ? '<span class="badge bg-success">فعال</span>'
            : '<span class="badge bg-danger">غیرفعال</span>';
    },

    upper(value) {
        return value === null || value === undefined
            ? ''
            : String(value).toUpperCase();
    },

    lower(value) {
        return value === null || value === undefined
            ? ''
            : String(value).toLowerCase();
    },

    truncate(value, length = 30) {

        if (value === null || value === undefined) return '';

        value = String(value);

        return value.length > length
            ? value.substring(0, length) + '…'
            : value;
    }
};

// Register custom helper from application code
Tech.addTemplateHelper = function (name, fn) {
    Tech.TemplateHelpers[name] = fn;
};

function compileTemplate(template) {

    const code = [];

    code.push('let __out = "";');

    let cursor = 0;

    const regex = /\{\{([\s\S]*?)\}\}/g;

    let match;

    while ((match = regex.exec(template)) !== null) {

        const text = template.slice(cursor, match.index);

        if (text) {
            code.push(`__out += ${JSON.stringify(text)};`);
        }

        let expr = match[1].trim();

        // Explicit output: {{= expression }}
        if (expr.startsWith('=')) {

            code.push(`__out += (${expr.substring(1).trim()} ?? "");`);
        }
        else if (isCodeBlock(expr)) {

            // Raw JS block: if/else/for...
            code.push(expr);
        }
        else {

            // Simple expression: {{ Id }} behaves like {{= Id }}
            code.push(`__out += (${expr} ?? "");`);
        }

        cursor = match.index + match[0].length;
    }

    const remaining = template.slice(cursor);

    if (remaining) {
        code.push(`__out += ${JSON.stringify(remaining)};`);
    }

    code.push('return __out;');

    const body = `
        const ctx = {};

        for (const k in row) {

            ctx[k] = row[k];

            const pascal = k.charAt(0).toUpperCase() + k.slice(1);

            ctx[pascal] = row[k];
        }

        Object.assign(ctx, Tech.TemplateHelpers);

        with (ctx) {
            ${code.join('\n')}
        }
    `;

    return new Function('row', body);
}

function isCodeBlock(expr) {

    if (!expr) return false;

    if (/[;{}]/.test(expr)) return true;

    return /\b(if|else|for|while|switch|case|default|break|continue|const|let|var|function|return|try|catch|finally|throw|new|typeof|await)\b/
        .test(expr);
}

const templateCache = new Map();

function applyTemplate(template, row) {

    if (!template) return '';

    let fn = templateCache.get(template);

    if (!fn) {

        fn = compileTemplate(template);

        templateCache.set(template, fn);
    }

    try {
        return fn(row);
    }
    catch (err) {

        console.error('TechUI template error:', err);

        return '<span class="text-danger">Template Error</span>';
    }
}

// ============================================================
// Component registry
// ============================================================

Tech.Components = new Map();
Tech.GridOptions = Tech.GridOptions || {};

Tech.register = function (name, factory) {
    Tech.Components.set(name, factory);
};

Tech.boot = function () {

    document.querySelectorAll('[data-tech-component]')
        .forEach(function (el) {

            const name = el.dataset.techComponent;
            const factory = Tech.Components.get(name);

            if (factory) {
                factory(el);
            }
        });

    Tech.wireValidation(document);
};

// ============================================================
// Popups (shared, one listener for the whole page)
// ============================================================

function closeTechPopups(except) {

    document.querySelectorAll('.tech-filter-popup, .tech-column-popup')
        .forEach(function (popup) {

            if (popup !== except) {
                popup.remove();
            }
        });
}

function positionPopup(popup, anchor, preferredWidth) {

    const rect = anchor.getBoundingClientRect();

    const margin = 8;

    const width = Math.min(preferredWidth, window.innerWidth - margin * 2);

    const isRtl = getComputedStyle(anchor).direction === 'rtl';

    let left = isRtl ? rect.left : rect.right - width;

    const maxLeft = window.innerWidth - width - margin;

    if (left > maxLeft) left = maxLeft;
    if (left < margin) left = margin;

    const gap = 6;

    let top = rect.bottom + gap;

    popup.style.position = 'fixed';
    popup.style.width = width + 'px';
    popup.style.left = left + 'px';
    popup.style.top = top + 'px';
    popup.style.zIndex = '2000';

    const height = popup.offsetHeight;

    if (height > 0 &&
        top + height > window.innerHeight - margin &&
        rect.top - height - gap >= margin) {

        popup.style.top = (rect.top - height - gap) + 'px';
    }

    return popup;
}

// ------------------------------------------------------------
// Floating panels (dropdown / datepicker / timepicker)
//
// Bootstrap modals scroll (overflow-y:auto) and clip their
// descendants, so panelled components switch to position:fixed
// while open. The panel stays in its original DOM position so
// outside-click and root.contains() checks keep working.
// ------------------------------------------------------------

function techFloatPopup(root, panel, options) {

    if (!root || !panel) return;

    options = options || {};

    if (root._techFloat && root._techFloat.panel === panel) {
        techPositionPopup(root);
        return;
    }

    techReleasePopup(root);

    const state = {
        panel: panel,
        anchor: options.anchor || root,
        width: options.width || null,
        savedStyle: panel.getAttribute('style'),
        scrollTargets: []
    };

    state.onScroll = function () { techPositionPopup(root); };

    root._techFloat = state;

    // Element scroll events do not reach window listeners (even in
    // capture), so bind to every scrollable ancestor (modal-body,
    // .modal, custom scroll containers).
    let scrollParent = root.parentElement;

    while (scrollParent && scrollParent !== document.body) {

        const style = window.getComputedStyle(scrollParent);

        if (/(auto|scroll|overlay)/.test(style.overflowY) ||
            /(auto|scroll|overlay)/.test(style.overflowX)) {

            scrollParent.addEventListener('scroll', state.onScroll);
            state.scrollTargets.push(scrollParent);
        }

        scrollParent = scrollParent.parentElement;
    }

    window.addEventListener('scroll', state.onScroll, true);
    window.addEventListener('resize', state.onScroll);

    techPositionPopup(root);
}

function techPositionPopup(root) {

    const state = root ? root._techFloat : null;

    if (!state) return;

    const panel = state.panel;
    const anchor = state.anchor;

    if (!panel.isConnected || !anchor.isConnected) {
        techReleasePopup(root);
        return;
    }

    const anchorRect = anchor.getBoundingClientRect();

    if (anchorRect.width === 0 && anchorRect.height === 0) return;

    const margin = 8;
    const gap = 4;

    panel.style.position = 'fixed';
    panel.style.zIndex = '2000';
    panel.style.right = 'auto';
    panel.style.insetInlineStart = 'auto';
    panel.style.left = '0px';
    panel.style.top = '0px';

    let width = null;

    if (state.width === 'anchor') width = anchorRect.width;
    else if (typeof state.width === 'number') width = state.width;

    panel.style.width = width ? width + 'px' : '';

    const rect = panel.getBoundingClientRect();
    const maxHeight = window.innerHeight - margin * 2;

    let top = anchorRect.bottom + gap;

    if (top + rect.height > window.innerHeight - margin) {

        const above = anchorRect.top - gap - rect.height;

        top = above >= margin ? above : Math.max(margin, window.innerHeight - margin - rect.height);
    }

    if (rect.height > maxHeight) {

        top = margin;
        panel.style.maxHeight = maxHeight + 'px';
        panel.style.overflowY = 'auto';
    }
    else {
        panel.style.maxHeight = '';
        panel.style.overflowY = '';
    }

    let left = anchorRect.left;

    if (left + rect.width > window.innerWidth - margin) {
        left = window.innerWidth - margin - rect.width;
    }

    if (left < margin) left = margin;

    panel.style.left = left + 'px';
    panel.style.top = top + 'px';
}

function techReleasePopup(root) {

    const state = root ? root._techFloat : null;

    if (!state) return;

    state.scrollTargets.forEach(function (target) {
        target.removeEventListener('scroll', state.onScroll);
    });

    window.removeEventListener('scroll', state.onScroll, true);
    window.removeEventListener('resize', state.onScroll);

    root._techFloat = null;

    const panel = state.panel;

    if (!panel) return;

    if (state.savedStyle === null) {
        panel.removeAttribute('style');
    }
    else {
        panel.setAttribute('style', state.savedStyle);
    }
}

document.addEventListener('click', function () {
    closeTechPopups(null);
});

// ============================================================
// Grid
// ============================================================

Tech.Grid = Tech.Grid || {};
Tech.Grid.instances = new Map();

Tech.Grid.get = function (id) {
    return Tech.Grid.instances.get(id);
};

Tech.register('grid', function (el) {

    if (el._techGrid) return;

    const options = Tech.GridOptions[el.id] || {};

    const columns = (options.columns || []).map(function (column, index) {

        return {
            key: column.key || column.field || ('col_' + (index + 1)),
            field: column.field || '',
            title: column.title || '',
            dataType: column.dataType || null,
            width: column.width || null,
            sortable: column.sortable !== false,
            filterable: column.filterable !== false,
            isTemplate: !!column.isTemplate,
            template: column.template || '',
            pinned: !!column.pinned,
            hidden: false
        };
    });

    const summaryItems = Array.isArray(options.summary) ? options.summary : [];

    let groupItems = normalizeGroups(options.groups);

    const toolbarItems = options.toolbar || [];

    const chatOptions = options.chat || null;

    const state = {
        page: 1,
        pageSize: parseInt(el.dataset.pageSize || '10', 10) || 10,
        sorts: [],
        filters: [],
        selectedIndex: -1,
        selectedIndices: new Set(),
        collapsedGroups: new Set()
    };

    const url = el.dataset.url;

    const httpMethod = (el.dataset.method || 'GET').toUpperCase();

    const sortableEnabled = (el.dataset.sortable || 'true') === 'true';
    const filterableEnabled = (el.dataset.filterable || 'true') === 'true';
    const pageableEnabled = (el.dataset.pageable || 'true') === 'true';
    const selectableEnabled = (el.dataset.selectable || 'false') === 'true';
    const multiSelectEnabled = (el.dataset.multiSelect || 'false') === 'true';
    const resizableEnabled = (el.dataset.resizable || 'false') === 'true';
    const reorderableEnabled = (el.dataset.reorderable || 'false') === 'true';
    const infiniteEnabled = (el.dataset.infiniteScroll || 'false') === 'true';

    const infiniteThreshold = 160;

    const gridLabel = el.dataset.label || 'جدول داده‌ها';

    const pageSizeSelectorEnabled =
        (el.dataset.pageSizeSelector || 'false') === 'true';

    const pageSizeOptions = (el.dataset.pageSizes || '10,20,50,100')
        .split(',')
        .map(function (value) { return parseInt(value, 10); })
        .filter(function (value) { return !isNaN(value) && value > 0; });

    const dataFunctionName = el.dataset.dataFunction;

    const height = parseInt(el.dataset.height || '0', 10) || 0;

    const groupPanelEnabled =
        (el.dataset.groupPanel || 'false') === 'true';

    const storageKey = `tech-grid-columns-${el.id}`;
    const widthStorageKey = `tech-grid-widths-${el.id}`;
    const pageSizeStorageKey = `tech-grid-pagesize-${el.id}`;
    const orderStorageKey = `tech-grid-order-${el.id}`;
    const groupStorageKey = `tech-grid-groups-${el.id}`;

    let currentRows = [];
    let lastResult = {
        data: [],
        total: 0,
        rowTotal: 0,
        grouped: false,
        page: 1,
        pageSize: state.pageSize,
        summary: []
    };
    let loading = false;
    let loadingStartedAt = 0;
    let loadingMore = false;
    let infiniteDone = false;
    let loadToken = 0;
    let pendingFocusField = null;
    let pendingGroupFocusKey = null;

    const chatState = {
        open: false,
        busy: false,
        focusInput: false,
        messages: [],
        highlight: null
    };

    // --------------------------------------------------------
    // Persisted state
    // --------------------------------------------------------

    function loadColumnState() {

        try {

            const raw = localStorage.getItem(storageKey);

            if (!raw) return;

            const hiddenKeys = JSON.parse(raw);

            if (!Array.isArray(hiddenKeys)) return;

            columns.forEach(function (column) {
                column.hidden = hiddenKeys.indexOf(column.key) >= 0;
            });

            if (columns.length > 0 && columns.every(function (column) {
                return column.hidden;
            })) {
                columns[0].hidden = false;
            }
        }
        catch {
            // ignore corrupt storage
        }
    }

    function saveColumnState() {

        try {

            const hiddenKeys = columns
                .filter(function (column) { return column.hidden; })
                .map(function (column) { return column.key; });

            localStorage.setItem(storageKey, JSON.stringify(hiddenKeys));
        }
        catch {
            // ignore
        }
    }

    function loadColumnWidths() {

        try {

            const raw = localStorage.getItem(widthStorageKey);

            if (!raw) return;

            const widths = JSON.parse(raw);

            columns.forEach(function (column) {

                const width = widths[column.key];

                if (width) column.width = width;
            });
        }
        catch {
            // ignore
        }
    }

    function saveColumnWidths() {

        try {

            const widths = {};

            columns.forEach(function (column) {

                if (column.width) widths[column.key] = column.width;
            });

            localStorage.setItem(widthStorageKey, JSON.stringify(widths));
        }
        catch {
            // ignore
        }
    }

    function loadPageSize() {

        try {

            const raw = localStorage.getItem(pageSizeStorageKey);

            if (!raw) return;

            const parsed = parseInt(raw, 10);

            if (!isNaN(parsed) && parsed > 0) state.pageSize = parsed;
        }
        catch {
            // ignore
        }
    }

    function loadColumnOrder() {

        if (!reorderableEnabled) return;

        try {

            const raw = localStorage.getItem(orderStorageKey);

            if (!raw) return;

            const keys = JSON.parse(raw);

            if (!Array.isArray(keys) || keys.length === 0) return;

            const ordered = [];

            keys.forEach(function (key) {
                const column = columns.find(function (item) {
                    return item.key === key;
                });

                if (column && ordered.indexOf(column) < 0) ordered.push(column);
            });

            columns.forEach(function (column) {
                if (ordered.indexOf(column) < 0) ordered.push(column);
            });

            columns.length = 0;

            columns.push.apply(columns, ordered);
        }
        catch {
            // ignore corrupt storage
        }
    }

    function saveColumnOrder() {

        try {
            localStorage.setItem(
                orderStorageKey,
                JSON.stringify(columns.map(function (column) {
                    return column.key;
                })));
        }
        catch {
            // ignore
        }
    }

    function loadGroupState() {

        if (!groupPanelEnabled) return;

        try {

            const raw = localStorage.getItem(groupStorageKey);

            if (raw === null) return;

            const saved = JSON.parse(raw);

            if (!Array.isArray(saved)) return;

            groupItems = normalizeGroups(saved);
        }
        catch {
            // ignore corrupt storage
        }
    }

    function saveGroupState() {

        if (!groupPanelEnabled) return;

        try {

            localStorage.setItem(
                groupStorageKey,
                JSON.stringify(groupItems.map(function (group) {
                    return { field: group.field, title: group.title };
                })));
        }
        catch {
            // ignore
        }
    }

    loadColumnState();
    loadColumnOrder();
    loadColumnWidths();
    loadPageSize();
    loadGroupState();

    (Array.isArray(options.sorts) ? options.sorts : []).forEach(function (sort) {

        if (!sort || !sort.field) return;

        const column = columns.find(function (item) {
            return item.field === sort.field;
        });

        if (!column || !sortableEnabled || !column.sortable) return;

        state.sorts.push({
            field: column.field,
            dir: sort.dir === 'desc' ? 'desc' : 'asc'
        });
    });

    function triggerGridEvent(name, payload) {

        try {
            Tech.trigger(el, name, payload);
        }
        catch (err) {
            console.error(`TechUI Grid ${name} handler error:`, err);
        }
    }

    // --------------------------------------------------------
    // Loading overlay
    // --------------------------------------------------------

    function showLoading() {

        loadingStartedAt = Date.now();

        el.setAttribute('aria-busy', 'true');

        const overlay = el.querySelector('.tech-grid-loading');

        if (overlay) overlay.classList.add('show');
    }

    async function hideLoading() {

        const overlay = el.querySelector('.tech-grid-loading');

        const elapsed = Date.now() - loadingStartedAt;

        if (elapsed < 200) {
            await new Promise(function (resolve) {
                setTimeout(resolve, 200 - elapsed);
            });
        }

        if (overlay) overlay.classList.remove('show');

        el.setAttribute('aria-busy', 'false');
    }

    // --------------------------------------------------------
    // Request
    // --------------------------------------------------------

    function buildParams(page) {

        const params = new URLSearchParams();

        params.append('Page', page || state.page);
        params.append('PageSize', state.pageSize);

        state.sorts.forEach(function (sort, index) {

            params.append(`Sorts[${index}].Field`, sort.field);
            params.append(`Sorts[${index}].Dir`, sort.dir);
        });

        groupItems.forEach(function (group, index) {

            params.append(`Groups[${index}].Field`, group.field);

            if (group.title) {
                params.append(`Groups[${index}].Title`, group.title);
            }
        });

        params.append('Filter.Logic', 'and');

        state.filters.forEach(function (filter, index) {

            params.append(`Filter.Filters[${index}].Field`, filter.field);
            params.append(`Filter.Filters[${index}].Operator`, filter.operator);

            if (filter.value !== null && filter.value !== undefined) {
                params.append(`Filter.Filters[${index}].Value`, filter.value);
            }
        });

        summaryItems.forEach(function (item, index) {

            if (!item) return;

            if (item.field) {
                params.append(`Summary[${index}].Field`, item.field);
            }

            params.append(`Summary[${index}].Type`, item.type || 'sum');
        });

        if (dataFunctionName) {

            const fn = window[dataFunctionName];

            if (typeof fn === 'function') {

                const extra = fn() || {};

                Object.keys(extra).forEach(function (key) {

                    const value = extra[key];

                    if (value !== null && value !== undefined && value !== '') {
                        params.append(key, value);
                    }
                });
            }
        }

        return params;
    }

    function normalizeResult(result) {

        result = result || {};

        return {
            data: result.data || result.Data || [],
            total: Number(result.total ?? result.Total ?? 0) || 0,
            rowTotal: Number(
                result.rowTotal ?? result.RowTotal ??
                result.total ?? result.Total ?? 0) || 0,
            grouped: !!(result.grouped ?? result.Grouped),
            page: Number(result.page ?? result.Page ?? state.page) || 1,
            pageSize: Number(result.pageSize ?? result.PageSize ?? state.pageSize) || state.pageSize,
            summary: result.summary || result.Summary || []
        };
    }

    async function fetchResult(page) {

        return normalizeResult(await Tech.requestJson(url, {
            method: httpMethod,
            params: buildParams(page),
            antiforgeryElement: el
        }));
    }

    async function load() {

        if (loading) return;

        if (!url) {
            renderError('آدرس منبع داده تنظیم نشده است.');
            return;
        }

        loadToken++;
        loading = true;
        loadingMore = false;
        infiniteDone = false;
        showLoading();

        triggerGridEvent('Binding', { sender: api });

        try {

            let result = await fetchResult();

            const totalPages = Math.max(
                Math.ceil(result.total / state.pageSize), 1);

            if (state.page > totalPages) {

                state.page = totalPages;

                result = await fetchResult();
            }

            if (result.page > 0) state.page = result.page;
            if (result.pageSize > 0) state.pageSize = result.pageSize;

            lastResult = result;
            currentRows = result.data;

            state.selectedIndex = -1;
            state.selectedIndices.clear();

            if (infiniteEnabled) {

                infiniteDone = state.page >= Math.max(
                    Math.ceil(lastResult.total / state.pageSize), 1);
            }

            render();

            triggerGridEvent('Bound', { sender: api, data: result });
        }
        catch (err) {

            console.error('TechUI Grid error:', err);

            renderError('خطا در بارگذاری داده‌ها.');

            triggerGridEvent('Error', { sender: api, error: err });
        }
        finally {

            loading = false;

            await hideLoading();

            if (infiniteEnabled) scheduleInfiniteCheck();
        }
    }

    async function loadMore() {

        if (!infiniteEnabled || loading || loadingMore ||
            infiniteDone || !url) {
            return;
        }

        const totalPages = Math.max(
            Math.ceil(lastResult.total / state.pageSize), 1);

        if (state.page >= totalPages) {

            infiniteDone = true;
            updateInfiniteFooter();
            return;
        }

        loadingMore = true;

        const token = loadToken;

        updateInfiniteFooter();

        try {

            const result = await fetchResult(state.page + 1);

            if (token !== loadToken) return;

            const rows = result.data || [];

            if (rows.length === 0) {

                infiniteDone = true;
            }
            else {

                const baseIndex = currentRows.length;

                currentRows = currentRows.concat(rows);

                state.page = result.page > 0 ? result.page : state.page + 1;

                lastResult = result;

                appendRows(rows, baseIndex);

                if (state.page >= Math.max(
                    Math.ceil(lastResult.total / state.pageSize), 1)) {
                    infiniteDone = true;
                }

                triggerGridEvent('Bound', {
                    sender: api,
                    data: result,
                    appended: true,
                    loaded: currentRows.length
                });
            }
        }
        catch (err) {

            console.error('TechUI Grid load more error:', err);

            triggerGridEvent('Error', { sender: api, error: err });
        }
        finally {

            loadingMore = false;

            if (token === loadToken) {

                updateInfiniteFooter();
                scheduleInfiniteCheck();
            }
        }
    }

    function infiniteNearBottom() {

        if (!infiniteEnabled || infiniteDone || loadingMore) return false;

        const body = el.querySelector('.tech-grid-body');

        if (!body) return false;

        if (height > 0) {

            return body.scrollHeight - body.scrollTop - body.clientHeight <=
                infiniteThreshold;
        }

        const sentinel = body.querySelector('.tech-grid-infinite-sentinel');

        if (!sentinel) return false;

        return sentinel.getBoundingClientRect().top <=
            window.innerHeight + infiniteThreshold;
    }

    function scheduleInfiniteCheck() {

        if (!infiniteEnabled) return;

        setTimeout(function () {

            if (infiniteNearBottom()) loadMore();

        }, 0);
    }

    // --------------------------------------------------------
    // Rendering
    // --------------------------------------------------------

    function visibleColumns() {

        return columns.filter(function (column) {
            return !column.hidden;
        });
    }

    function pinnedColumns() {

        return visibleColumns().filter(function (column) {
            return column.pinned;
        });
    }

    function pinSelectColumn() {

        return multiSelectEnabled && pinnedColumns().length > 0;
    }

    function getValue(row, field) {

        if (!row || !field) return '';

        let value = row;

        const segments = String(field).split('.');

        for (let i = 0; i < segments.length; i++) {

            value = getProperty(value, segments[i]);

            if (value === null || value === undefined) return '';
        }

        return value;
    }

    function findFilter(field) {

        return state.filters.find(function (filter) {
            return filter.field === field;
        });
    }

    function renderError(message) {

        const body = el.querySelector('.tech-grid-body');

        const html =
            '<div class="alert alert-danger m-2 mb-0" role="alert">' +
            Tech.escapeHtml(message) +
            '</div>';

        if (body) {
            body.innerHTML = html;
        }
        else {
            el.insertAdjacentHTML('beforeend', html);
        }
    }

    function buildToolbarHtml() {

        const groupPanelVisible = groupPanelEnabled &&
            groupableColumns().length > 0;

        if (toolbarItems.length === 0 && !chatOptions && !groupPanelVisible) {
            return '';
        }

        let html =
            '<div class="tech-grid-toolbar d-flex align-items-center gap-2 p-2 border-bottom flex-wrap">';

        toolbarItems.forEach(function (item) {

            const icon = item.icon
                ? `<i class="bi bi-${Tech.escapeHtml(item.icon)} me-1"></i>`
                : '';

            html +=
                '<button type="button" ' +
                'class="btn btn-sm btn-outline-primary tech-toolbar-btn" ' +
                `data-action="${Tech.escapeHtml(item.type || 'custom')}" ` +
                `data-handler="${Tech.escapeHtml(item.handler || '')}" ` +
                `data-file-name="${Tech.escapeHtml(item.fileName || '')}">` +
                icon + Tech.escapeHtml(item.text || '') +
                '</button>';
        });

        if (groupPanelVisible) {

            html +=
                '<button type="button" ' +
                'class="btn btn-sm btn-outline-primary tech-toolbar-btn tech-grid-group-btn" ' +
                'data-action="groups" aria-haspopup="dialog" ' +
                `aria-expanded="${groupPanel.open ? 'true' : 'false'}">` +
                '<i class="bi bi-collection me-1"></i>گروه‌بندی' +
                '</button>';
        }

        if (chatOptions) {

            html +=
                '<button type="button" ' +
                'class="btn btn-sm btn-primary tech-toolbar-btn tech-grid-chat-toggle ms-auto" ' +
                'data-action="chat" aria-haspopup="dialog" ' +
                `aria-expanded="${chatState.open ? 'true' : 'false'}">` +
                `<i class="bi bi-${Tech.escapeHtml(chatOptions.icon || 'chat-dots')} me-1"></i>` +
                Tech.escapeHtml(chatOptions.buttonText || 'دستیار') +
                '</button>';
        }

        return html + '</div>';
    }

    const filterShortLabels = {
        contains: 'شامل',
        notcontains: 'شامل نیست',
        eq: '=',
        neq: '≠',
        startswith: 'شروع با',
        endswith: 'پایان با',
        gt: '>',
        gte: '≥',
        lt: '<',
        lte: '≤',
        isnull: 'خالی است',
        isnotnull: 'خالی نیست',
        in: 'یکی از',
        notin: 'هیچ‌کدام'
    };

    function filterColumnTitle(field) {

        const column = columns.find(function (item) {
            return item.field === field;
        });

        return column ? column.title : field;
    }

    function filterValueText(filter) {

        if (isNullaryOperator(filter.operator)) return '';

        const column = columns.find(function (item) {
            return item.field === filter.field;
        });

        const value = String(filter.value ?? '');

        if (column && column.dataType === 'boolean') {
            return value === 'true' ? 'بله' : value === 'false' ? 'خیر' : value;
        }

        return value;
    }

    function buildFilterBarHtml() {

        if (state.filters.length === 0) return '';

        let html =
            '<div class="tech-grid-filterbar" role="region" ' +
            'aria-label="فیلترهای فعال" aria-live="polite">' +
            '<span class="tech-grid-filterbar-label">فیلترها:</span>';

        state.filters.forEach(function (filter) {

            const title = filterColumnTitle(filter.field);

            const operatorText = filterShortLabels[filter.operator] || filter.operator;

            const valueText = filterValueText(filter);

            html +=
                '<span class="tech-filter-chip">' +
                `<span class="tech-filter-chip-title">${Tech.escapeHtml(title)}</span>` +
                `<span class="tech-filter-chip-body">${Tech.escapeHtml(operatorText)}` +
                (valueText ? ' ' + Tech.escapeHtml(valueText) : '') +
                '</span>' +
                '<button type="button" class="tech-filter-chip-remove" ' +
                `data-field="${Tech.escapeHtml(filter.field)}" ` +
                `aria-label="حذف فیلتر ${Tech.escapeHtml(title)}" title="حذف فیلتر">` +
                '<i class="bi bi-x"></i></button>' +
                '</span>';
        });

        html +=
            '<button type="button" class="tech-filter-clear-all">' +
            'پاک کردن همه</button></div>';

        return html;
    }

    function buildHeaderHtml() {

        let html = '<thead><tr>';

        if (multiSelectEnabled) {

            const pinnedClass = pinSelectColumn() ? ' tech-col-pinned' : '';

            html +=
                `<th class="tech-grid-select-col${pinnedClass}" scope="col">` +
                '<input type="checkbox" class="tech-grid-select-all" ' +
                'aria-label="انتخاب همه‌ی سطرها"></th>';
        }

        columns.forEach(function (column) {

            if (column.hidden) return;

            const widthStyle = column.width
                ? `width:${column.width}px;`
                : '';

            const sortIndex = state.sorts.findIndex(function (sort) {
                return sort.field === column.field;
            });

            const sort = sortIndex >= 0 ? state.sorts[sortIndex] : null;

            let icon = '';

            if (sort) {

                icon = sort.dir === 'asc'
                    ? '<i class="bi bi-sort-down-alt ms-1"></i>'
                    : '<i class="bi bi-sort-down ms-1"></i>';

                icon += `<sup class="tech-sort-index">${sortIndex + 1}</sup>`;
            }

            const canSort = sortableEnabled && column.sortable && column.field;

            const activeFilter = findFilter(column.field);

            const canFilter = filterableEnabled &&
                column.filterable &&
                !!column.field;

            const filterButton = canFilter
                ? '<button type="button" ' +
                  `class="btn btn-sm p-0 ms-1 tech-filter-btn ${activeFilter ? 'text-primary' : 'text-muted'}" ` +
                  `data-field="${Tech.escapeHtml(column.field)}" ` +
                  `aria-label="فیلتر ${Tech.escapeHtml(column.title)}" ` +
                  'aria-haspopup="dialog" title="فیلتر">' +
                  '<i class="bi bi-funnel-fill"></i></button>'
                : '';

            const resizeHandle = resizableEnabled
                ? `<span class="tech-resize-handle" data-key="${Tech.escapeHtml(column.key)}"></span>`
                : '';

            const reorderHandle = reorderableEnabled
                ? '<span class="tech-reorder-handle" draggable="true" ' +
                  `data-key="${Tech.escapeHtml(column.key)}" ` +
                  'title="جابه‌جایی ستون" aria-hidden="true">' +
                  '<i class="bi bi-grip-vertical"></i></span>'
                : '';

            const sortClass = canSort ? 'tech-sortable' : '';
            const sortCursor = canSort ? 'cursor:pointer;' : '';
            const sortAttrs = canSort ? 'role="button" tabindex="0"' : '';
            const sortTitle = canSort
                ? 'title="مرتب‌سازی — برای چندستونه Shift+کلیک"'
                : '';
            const ariaSort = canSort
                ? `aria-sort="${!sort
                    ? 'none'
                    : sort.dir === 'asc' ? 'ascending' : 'descending'}"`
                : '';

            const pinnedClass = column.pinned ? ' tech-col-pinned' : '';

            html +=
                `<th data-key="${Tech.escapeHtml(column.key)}" ` +
                `class="tech-col${pinnedClass}" scope="col" ${ariaSort} style="${widthStyle}">` +
                reorderHandle +
                '<div class="d-flex align-items-center justify-content-between gap-1">' +
                `<span class="${sortClass} flex-grow-1" ${sortAttrs} ${sortTitle} ` +
                `data-field="${Tech.escapeHtml(column.field)}" ` +
                `style="${sortCursor}">` +
                Tech.escapeHtml(column.title) + icon +
                '</span>' +
                filterButton +
                '</div>' +
                resizeHandle +
                '</th>';
        });

        return html + '</tr></thead>';
    }

    function buildDataRowHtml(row, rowIndex, visible, highlight, highlightFields) {

        const multiSelected = multiSelectEnabled &&
            state.selectedIndices.has(rowIndex);

        const selectedClass = (rowIndex === state.selectedIndex || multiSelected)
            ? ' tech-row-selected'
            : '';

        const selectableClass = selectableEnabled
            ? ' tech-grid-row-selectable'
            : '';

        const highlightClass = highlight && highlight.value !== '' &&
            highlightFields.some(function (field) {
                return sameCellValue(getValue(row, field), highlight.value);
            })
            ? ' tech-grid-row-highlight'
            : '';

        const isSelected = rowIndex === state.selectedIndex || multiSelected;

        const selectableAttrs = selectableEnabled
            ? ` aria-selected="${isSelected}"`
            : '';

        let html = `<tr class="tech-grid-row${selectableClass}${selectedClass}${highlightClass}" ` +
            `data-row-index="${rowIndex}"${selectableAttrs}>`;

        if (multiSelectEnabled) {

            const pinnedClass = pinSelectColumn() ? ' tech-col-pinned' : '';

            html +=
                `<td class="tech-grid-select-col${pinnedClass}">` +
                `<input type="checkbox" class="tech-grid-row-check" data-index="${rowIndex}" ` +
                `aria-label="انتخاب سطر ${rowIndex + 1}" ${multiSelected ? 'checked' : ''}>` +
                '</td>';
        }

        visible.forEach(function (column, columnIndex) {

            const cellTabIndex = rowIndex === 0 && columnIndex === 0
                ? '0'
                : '-1';

            const pinnedClass = column.pinned ? ' class="tech-col-pinned"' : '';

            const cellAttrs =
                ` data-key="${Tech.escapeHtml(column.key)}"` +
                pinnedClass +
                ` tabindex="${cellTabIndex}"`;

            if (column.isTemplate) {

                html += '<td' + cellAttrs + '>' +
                    applyTemplate(column.template, row) + '</td>';
            }
            else {

                html += '<td' + cellAttrs + '>' +
                    Tech.escapeHtml(getValue(row, column.field)) + '</td>';
            }
        });

        return html + '</tr>';
    }

    function groupColumn(field) {

        const normalized = String(field).toLowerCase();

        return columns.find(function (column) {
            return column.field &&
                column.field.toLowerCase() === normalized;
        }) || null;
    }

    function normalizeGroups(input) {

        const groups = [];

        (Array.isArray(input) ? input : []).forEach(function (group) {

            const field = typeof group === 'string'
                ? group
                : (group && group.field);

            if (!field) return;

            const value = String(field).trim();

            if (!value) return;

            const duplicate = groups.some(function (item) {
                return item.field.toLowerCase() === value.toLowerCase();
            });

            if (duplicate) return;

            const column = groupColumn(value);

            groups.push({
                field: column ? column.field : value,
                title: (group && group.title) || ''
            });
        });

        return groups;
    }

    function applyGroups(groups) {

        groupItems = normalizeGroups(groups);

        state.collapsedGroups.clear();
        state.page = 1;

        saveGroupState();

        render();

        if (url) {
            load();
        }

        if (groupPanel.open) {

            const button = el.querySelector('.tech-grid-group-btn');

            if (button) button.setAttribute('aria-expanded', 'true');
        }
    }

    function groupTitle(group) {

        if (group.title) return group.title;

        const column = groupColumn(group.field);

        return column ? column.title : group.field;
    }

    function formatGroupValue(value, column) {

        if (value === null || value === undefined || value === '') {
            return '<span class="tech-group-empty">(خالی)</span>';
        }

        if (typeof value === 'boolean' ||
            (column && column.dataType === 'boolean')) {

            const truthy = value === true ||
                String(value).toLowerCase() === 'true';

            return truthy ? 'بله' : 'خیر';
        }

        return Tech.escapeHtml(String(value));
    }

    function groupFullKey(parentKey, field, rawKey) {

        return (parentKey ? parentKey + '\u0001' : '') +
            field + '=' + rawKey;
    }

    function buildGroupRowHtml(group, level, key, value, count, collapsed, colspan) {

        const escapedKey = Tech.escapeHtml(key);

        const indent = 0.75 + level * 1.25;

        return '<tr class="tech-grid-group-row' +
            (collapsed ? ' tech-grid-group-collapsed' : '') + '" ' +
            `data-group-key="${escapedKey}" data-group-level="${level}">` +
            `<td colspan="${colspan}" class="tech-grid-group-cell" ` +
            `style="padding-inline-start:${indent}rem">` +
            '<button type="button" class="tech-grid-group-toggle" ' +
            `data-group-key="${escapedKey}" ` +
            `aria-expanded="${collapsed ? 'false' : 'true'}">` +
            '<i class="bi bi-chevron-down" aria-hidden="true"></i>' +
            `<span class="visually-hidden">${collapsed ? 'باز کردن گروه' : 'بستن گروه'}</span>` +
            '</button>' +
            `<span class="tech-group-title">${Tech.escapeHtml(groupTitle(group))}:</span>` +
            `<span class="tech-group-value">${formatGroupValue(value, groupColumn(group.field))}</span>` +
            `<span class="tech-group-count">(${count.toLocaleString()} ردیف)</span>` +
            '</td></tr>';
    }

    function buildGroupedRows(
        visible, highlight, highlightFields, colspan, sourceRows,
        indexOffset) {

        const rowsToRender = sourceRows || currentRows;
        const offset = indexOffset || 0;

        const rowIndexMap = new Map();

        rowsToRender.forEach(function (row, index) {
            if (!rowIndexMap.has(row)) rowIndexMap.set(row, offset + index);
        });

        function renderLevel(rows, level, parentKey) {

            const group = groupItems[level];

            const buckets = new Map();

            rows.forEach(function (row) {

                const value = getValue(row, group.field);

                const rawKey = value === '' ? '' : String(value);

                if (!buckets.has(rawKey)) {
                    buckets.set(rawKey, { value: value, rows: [] });
                }

                buckets.get(rawKey).rows.push(row);
            });

            let html = '';

            buckets.forEach(function (bucket, rawKey) {

                const key = groupFullKey(parentKey, group.field, rawKey);

                const collapsed = state.collapsedGroups.has(key);

                html += buildGroupRowHtml(
                    group, level, key, bucket.value, bucket.rows.length,
                    collapsed, colspan);

                if (collapsed) return;

                if (level + 1 < groupItems.length) {

                    html += renderLevel(bucket.rows, level + 1, key);

                    return;
                }

                bucket.rows.forEach(function (row) {

                    html += buildDataRowHtml(
                        row, rowIndexMap.get(row), visible,
                        highlight, highlightFields);
                });
            });

            return html;
        }

        return renderLevel(rowsToRender, 0, '');
    }

    function allGroupKeys() {

        function collect(rows, level, parentKey) {

            if (level >= groupItems.length) return [];

            const group = groupItems[level];

            const buckets = new Map();

            rows.forEach(function (row) {

                const value = getValue(row, group.field);

                const rawKey = value === '' ? '' : String(value);

                if (!buckets.has(rawKey)) buckets.set(rawKey, []);

                buckets.get(rawKey).push(row);
            });

            const keys = [];

            buckets.forEach(function (bucketRows, rawKey) {

                const key = groupFullKey(parentKey, group.field, rawKey);

                keys.push(key);
                keys.push.apply(keys, collect(bucketRows, level + 1, key));
            });

            return keys;
        }

        return collect(currentRows, 0, '');
    }

    function buildBodyHtml() {

        const visible = visibleColumns();
        const colspan = Math.max(visible.length, 1) +
            (multiSelectEnabled ? 1 : 0);

        if (currentRows.length === 0) {

            return '<tbody><tr><td colspan="' + colspan + '">' +
                '<div class="tech-grid-empty">' +
                '<div class="tech-grid-empty-icon">📄</div>' +
                '<div class="fw-semibold mb-1">داده‌ای یافت نشد</div>' +
                '<div class="small">فیلترها را تغییر دهید یا داده جدید اضافه کنید.</div>' +
                '</div></td></tr></tbody>';
        }

        const highlight = chatState.highlight;

        const highlightFields = highlight
            ? (highlight.field
                ? [highlight.field]
                : visible.map(function (column) { return column.field; }))
            : [];

        let html = '<tbody>';

        if (groupItems.length === 0) {

            currentRows.forEach(function (row, rowIndex) {

                html += buildDataRowHtml(
                    row, rowIndex, visible, highlight, highlightFields);
            });
        }
        else {

            html += buildGroupedRows(
                visible, highlight, highlightFields, colspan);
        }

        return html + '</tbody>';
    }

    // --------------------------------------------------------
    // Grouping panel
    // --------------------------------------------------------

    const groupPanel = {
        open: false,
        element: null
    };

    function groupableColumns() {

        return columns.filter(function (column) {
            return !!column.field;
        });
    }

    function buildGroupLevelsHtml() {

        if (groupItems.length === 0) {

            return '<div class="tech-group-panel-empty">' +
                'گروه‌بندی فعال نیست.</div>';
        }

        let html = '';

        groupItems.forEach(function (group, index) {

            html +=
                `<div class="tech-group-level" data-field="${Tech.escapeHtml(group.field)}">` +
                `<span class="tech-group-level-index">${index + 1}</span>` +
                `<span class="tech-group-level-title">${Tech.escapeHtml(groupTitle(group))}</span>` +
                '<div class="tech-group-level-actions">' +
                '<button type="button" class="tech-group-up" ' +
                `aria-label="بالا بردن سطح ${Tech.escapeHtml(groupTitle(group))}" ` +
                `title="بالا"${index === 0 ? ' disabled' : ''}>` +
                '<i class="bi bi-arrow-up" aria-hidden="true"></i></button>' +
                '<button type="button" class="tech-group-down" ' +
                `aria-label="پایین آوردن سطح ${Tech.escapeHtml(groupTitle(group))}" ` +
                `title="پایین"${index === groupItems.length - 1 ? ' disabled' : ''}>` +
                '<i class="bi bi-arrow-down" aria-hidden="true"></i></button>' +
                '<button type="button" class="tech-group-remove" ' +
                `aria-label="حذف سطح ${Tech.escapeHtml(groupTitle(group))}" title="حذف">` +
                '<i class="bi bi-x" aria-hidden="true"></i></button>' +
                '</div></div>';
        });

        return html;
    }

    function buildGroupAddHtml() {

        const all = groupableColumns();

        const available = all.filter(function (column) {

            return !groupItems.some(function (group) {
                return group.field.toLowerCase() ===
                    column.field.toLowerCase();
            });
        });

        let html =
            '<select class="form-select form-select-sm tech-group-field" ' +
            'aria-label="فیلد گروه‌بندی"' +
            (available.length === 0 ? ' disabled' : '') + '>';

        if (available.length === 0) {

            html += '<option>' + (all.length === 0
                ? 'ستون قابل گروه‌بندی وجود ندارد'
                : 'همه‌ی ستون‌ها انتخاب شده‌اند') + '</option>';
        }
        else {

            available.forEach(function (column) {

                html +=
                    `<option value="${Tech.escapeHtml(column.field)}">` +
                    Tech.escapeHtml(column.title) + '</option>';
            });
        }

        return html + '</select>' +
            '<button type="button" class="btn btn-sm btn-primary tech-group-add"' +
            (available.length === 0 ? ' disabled' : '') + '>افزودن</button>';
    }

    function buildGroupPanelHtml() {

        return '<div class="tech-group-panel" role="dialog" ' +
            'aria-label="گروه‌بندی ردیف‌ها">' +
            '<div class="tech-group-panel-head">' +
            '<span class="tech-group-panel-title">گروه‌بندی ردیف‌ها</span>' +
            '<button type="button" class="tech-group-panel-close" ' +
            'aria-label="بستن پنل گروه‌بندی" title="بستن">' +
            '<i class="bi bi-x-lg" aria-hidden="true"></i></button>' +
            '</div>' +
            '<div class="tech-group-panel-list">' + buildGroupLevelsHtml() + '</div>' +
            '<div class="tech-group-panel-add">' + buildGroupAddHtml() + '</div>' +
            '<div class="tech-group-panel-foot">' +
            '<button type="button" class="btn btn-sm btn-outline-secondary tech-group-clear"' +
            (groupItems.length === 0 ? ' disabled' : '') + '>حذف گروه‌بندی</button>' +
            '</div></div>';
    }

    function groupPanelButton() {

        return el.querySelector('.tech-grid-group-btn');
    }

    function positionGroupPanel() {

        const button = groupPanelButton();

        if (!button || !groupPanel.element) return;

        const rect = button.getBoundingClientRect();
        const rtl = getComputedStyle(el).direction === 'rtl';
        const margin = 8;
        const gap = 6;
        const viewportWidth = document.documentElement.clientWidth;
        const viewportHeight = document.documentElement.clientHeight;
        const panelWidth = groupPanel.element.offsetWidth;

        if (rtl) {

            const preferred = viewportWidth - rect.right;
            const maxRight = viewportWidth - panelWidth - margin;

            groupPanel.element.style.right =
                Math.round(Math.max(margin, Math.min(preferred, Math.max(margin, maxRight)))) + 'px';
            groupPanel.element.style.left = 'auto';
        }
        else {

            const maxLeft = viewportWidth - panelWidth - margin;

            groupPanel.element.style.left =
                Math.round(Math.max(margin, Math.min(rect.left, Math.max(margin, maxLeft)))) + 'px';
            groupPanel.element.style.right = 'auto';
        }

        const spaceBelow = viewportHeight - rect.bottom - gap - margin;
        const spaceAbove = rect.top - gap - margin;

        if (spaceBelow < 220 && spaceAbove > spaceBelow) {

            groupPanel.element.style.top = 'auto';
            groupPanel.element.style.bottom =
                Math.round(viewportHeight - rect.top + gap) + 'px';
            groupPanel.element.style.maxHeight =
                Math.max(120, Math.round(spaceAbove)) + 'px';
        }
        else {

            groupPanel.element.style.bottom = 'auto';
            groupPanel.element.style.top =
                Math.round(rect.bottom + gap) + 'px';
            groupPanel.element.style.maxHeight =
                Math.max(120, Math.round(spaceBelow)) + 'px';
        }
    }

    function focusGroupControl(selector) {

        if (!groupPanel.open) return;

        const target = groupPanel.element.querySelector(selector);

        if (target) target.focus();
    }

    function refreshGroupPanel() {

        if (!groupPanel.open || !groupPanel.element) return;

        groupPanel.element.querySelector('.tech-group-panel-list').innerHTML =
            buildGroupLevelsHtml();
        groupPanel.element.querySelector('.tech-group-panel-add').innerHTML =
            buildGroupAddHtml();

        const clear = groupPanel.element.querySelector('.tech-group-clear');

        if (clear) clear.disabled = groupItems.length === 0;

        positionGroupPanel();
    }

    function addGroupField(field) {

        if (!field) return;

        if (groupItems.some(function (group) {
            return group.field.toLowerCase() === String(field).toLowerCase();
        })) {
            return;
        }

        applyGroups(groupItems.concat([{ field: field, title: '' }]));

        refreshGroupPanel();

        focusGroupControl('.tech-group-field');
    }

    function removeGroupAt(index) {

        if (index < 0 || index >= groupItems.length) return;

        applyGroups(groupItems.filter(function (_, itemIndex) {
            return itemIndex !== index;
        }));

        refreshGroupPanel();

        const rows = groupPanel.element
            ? groupPanel.element.querySelectorAll('.tech-group-level')
            : [];

        if (rows.length > 0) {

            const target = rows[Math.min(index, rows.length - 1)]
                .querySelector('.tech-group-remove');

            if (target) target.focus();
        }
        else {

            focusGroupControl('.tech-group-field');
        }
    }

    function moveGroup(index, delta) {

        const target = index + delta;

        if (target < 0 || target >= groupItems.length) return;

        const next = groupItems.slice();
        const moved = next.splice(index, 1)[0];

        next.splice(target, 0, moved);

        applyGroups(next);

        refreshGroupPanel();

        const rows = groupPanel.element
            ? groupPanel.element.querySelectorAll('.tech-group-level')
            : [];

        const control = rows[target] &&
            rows[target].querySelector('.tech-group-remove');

        if (control) control.focus();
    }

    function clearGroups() {

        applyGroups([]);

        refreshGroupPanel();

        focusGroupControl('.tech-group-field');
    }

    function onGroupPanelDocClick(ev) {

        if (!groupPanel.open) return;

        if (groupPanel.element && groupPanel.element.contains(ev.target)) return;

        const button = groupPanelButton();

        if (button && button.contains(ev.target)) return;

        closeGroupPanel();
    }

    function onGroupPanelKeydown(ev) {

        if (ev.key === 'Escape' && groupPanel.open) {

            ev.stopPropagation();
            ev.preventDefault();

            closeGroupPanel(true);
        }
    }

    function onGroupPanelViewportChange(ev) {

        if (!groupPanel.open) return;

        if (ev && ev.type === 'scroll' &&
            groupPanel.element &&
            ev.target instanceof Node &&
            groupPanel.element.contains(ev.target)) {
            return;
        }

        closeGroupPanel();
    }

    function openGroupPanel() {

        closeGroupPanel();

        const panel = document.createElement('div');

        panel.innerHTML = buildGroupPanelHtml();

        const element = panel.firstElementChild;

        const themeHost = el.closest('.tech-dark, [data-tech-theme="dark"]');

        if (themeHost) {

            if (themeHost.classList.contains('tech-dark')) {
                element.classList.add('tech-dark');
            }
            else {
                element.setAttribute('data-tech-theme', 'dark');
            }
        }

        if (getComputedStyle(el).direction === 'rtl') {
            element.classList.add('tech-rtl');
        }

        document.body.appendChild(element);

        groupPanel.element = element;
        groupPanel.open = true;

        const button = groupPanelButton();

        if (button) button.setAttribute('aria-expanded', 'true');

        positionGroupPanel();

        const initialFocus = element.querySelector('.tech-group-panel-close');

        if (initialFocus) initialFocus.focus({ preventScroll: true });

        element.addEventListener('click', function (ev) {

            const control = ev.target.closest('button');

            if (!control) return;

            if (control.classList.contains('tech-group-panel-close')) {
                closeGroupPanel(true);
                return;
            }

            const row = control.closest('.tech-group-level');

            const rows = Array.prototype.slice.call(
                element.querySelectorAll('.tech-group-level'));

            const index = row ? rows.indexOf(row) : -1;

            if (control.classList.contains('tech-group-add')) {

                const select = element.querySelector('.tech-group-field');

                if (select) addGroupField(select.value);

                return;
            }

            if (control.classList.contains('tech-group-clear')) {
                clearGroups();
                return;
            }

            if (index < 0) return;

            if (control.classList.contains('tech-group-up')) {
                moveGroup(index, -1);
            }
            else if (control.classList.contains('tech-group-down')) {
                moveGroup(index, 1);
            }
            else if (control.classList.contains('tech-group-remove')) {
                removeGroupAt(index);
            }
        });

        document.addEventListener('click', onGroupPanelDocClick, true);
        document.addEventListener('keydown', onGroupPanelKeydown, true);
        window.addEventListener('resize', onGroupPanelViewportChange);
        window.addEventListener('scroll', onGroupPanelViewportChange, true);
    }

    function closeGroupPanel(focusButton) {

        if (!groupPanel.open) return;

        if (groupPanel.element) groupPanel.element.remove();

        groupPanel.element = null;
        groupPanel.open = false;

        const button = groupPanelButton();

        if (button) {

            button.setAttribute('aria-expanded', 'false');

            if (focusButton) button.focus();
        }

        document.removeEventListener('click', onGroupPanelDocClick, true);
        document.removeEventListener('keydown', onGroupPanelKeydown, true);
        window.removeEventListener('resize', onGroupPanelViewportChange);
        window.removeEventListener('scroll', onGroupPanelViewportChange, true);
    }

    const summaryTypeLabels = {
        count: 'تعداد',
        sum: 'جمع',
        average: 'میانگین',
        min: 'کمینه',
        max: 'بیشینه'
    };

    function summaryColumn(item) {

        const field = String(item && item.field || '');

        if (!field) {
            return visibleColumns().find(function (column) {
                return !!column.field;
            }) || visibleColumns()[0] || null;
        }

        return columns.find(function (column) {
            return column.field &&
                column.field.toLowerCase() === field.toLowerCase();
        }) || null;
    }

    function summaryServerValue(item) {

        const field = String(item.field || '');
        const type = String(item.type || 'sum').toLowerCase();

        const list = Array.isArray(lastResult.summary) ? lastResult.summary : [];

        const match = list.find(function (entry) {

            if (!entry) return false;

            const entryField = String(entry.field ?? entry.Field ?? '');
            const entryType = String(entry.type ?? entry.Type ?? 'sum').toLowerCase();

            return entryType === type &&
                entryField.toLowerCase() === field.toLowerCase();
        });

        if (!match) return undefined;

        return Object.prototype.hasOwnProperty.call(match, 'value')
            ? match.value
            : match.Value;
    }

    function summaryNumber(value) {

        if (value === null || value === undefined || value === '') return null;

        if (typeof value === 'number') return isFinite(value) ? value : null;

        const cleaned = String(value)
            .replace(/[\s,٬]/g, '')
            .replace(/[^\d.+-]/g, '');

        const parsed = Number(cleaned);

        return isFinite(parsed) ? parsed : null;
    }

    function summaryFallback(item, column) {

        const type = String(item.type || 'sum').toLowerCase();
        const field = item.field || (column && column.field) || '';

        if (type === 'count') {

            if (!field) return currentRows.length;

            return currentRows.filter(function (row) {
                const value = getValue(row, field);
                return value !== null && value !== undefined && value !== '';
            }).length;
        }

        if (!field) return null;

        const numbers = currentRows
            .map(function (row) { return summaryNumber(getValue(row, field)); })
            .filter(function (value) { return value !== null; });

        if (numbers.length === 0) return null;

        if (type === 'sum') {
            return numbers.reduce(function (sum, value) { return sum + value; }, 0);
        }

        if (type === 'average') {
            return numbers.reduce(function (sum, value) { return sum + value; }, 0) /
                numbers.length;
        }

        if (type === 'min') return Math.min.apply(null, numbers);
        if (type === 'max') return Math.max.apply(null, numbers);

        return null;
    }

    function formatSummaryValue(value) {

        if (value === null || value === undefined || value === '') return '';

        const number = Number(value);

        if (!isFinite(number)) return Tech.escapeHtml(String(value));

        const fractionDigits = Math.abs(number - Math.round(number)) > 1e-9 ? 2 : 0;

        try {
            return number.toLocaleString('fa-IR', {
                maximumFractionDigits: fractionDigits
            });
        }
        catch {
            return number.toLocaleString();
        }
    }

    function buildFooterHtml() {

        if (summaryItems.length === 0) return '';

        const visible = visibleColumns();

        const entriesByKey = {};

        summaryItems.forEach(function (item) {

            if (!item) return;

            const column = summaryColumn(item);

            if (!column) return;

            if (!entriesByKey[column.key]) entriesByKey[column.key] = [];

            entriesByKey[column.key].push({ item: item, column: column });
        });

        if (Object.keys(entriesByKey).length === 0) return '';

        let html = '<tfoot class="tech-grid-footer"><tr>';

        if (multiSelectEnabled) {

            const pinnedClass = pinSelectColumn() ? ' tech-col-pinned' : '';

            html += `<td class="tech-grid-select-col${pinnedClass}"></td>`;
        }

        visible.forEach(function (column) {

            const pinnedClass = column.pinned ? ' tech-col-pinned' : '';

            const keyAttr = ` data-key="${Tech.escapeHtml(column.key)}"`;

            const entries = entriesByKey[column.key];

            if (!entries) {

                html += `<td class="tech-summary-empty${pinnedClass}"${keyAttr}></td>`;

                return;
            }

            let inner = '';

            entries.forEach(function (entry, index) {

                const type = String(entry.item.type || 'sum').toLowerCase();

                let value = summaryServerValue(entry.item);

                if (value === undefined) {
                    value = summaryFallback(entry.item, entry.column);
                }

                const label = entry.item.label || summaryTypeLabels[type] || '';

                if (index > 0) {
                    inner += '<span class="tech-summary-sep" aria-hidden="true">·</span>';
                }

                inner +=
                    (label
                        ? `<span class="tech-summary-label">${Tech.escapeHtml(label)}</span>`
                        : '') +
                    `<span class="tech-summary-value">${formatSummaryValue(value)}</span>`;
            });

            html +=
                `<td class="tech-summary-cell${pinnedClass}"${keyAttr} ` +
                `data-summary-field="${Tech.escapeHtml(column.field || '')}">` +
                inner + '</td>';
        });

        return html + '</tr></tfoot>';
    }

    function applyPinnedOffsets() {

        const body = el.querySelector('.tech-grid-body');
        const table = body ? body.querySelector('table') : null;

        const headers = table
            ? Array.from(table.querySelectorAll('thead th'))
            : [];

        if (!table || headers.length === 0) {
            el.removeAttribute('data-pin-side');
            return;
        }

        const rtl = getComputedStyle(el).direction === 'rtl';
        const side = rtl ? 'right' : 'left';
        const otherSide = rtl ? 'left' : 'right';

        const offsets = {};

        let offset = 0;
        let lastKey = null;

        headers.forEach(function (th) {

            if (!th.classList.contains('tech-col-pinned')) return;

            const key = th.dataset.key || '__select__';

            offsets[key] = offset;

            offset += th.getBoundingClientRect().width;

            lastKey = key;
        });

        if (lastKey === null) {
            el.removeAttribute('data-pin-side');
            return;
        }

        el.setAttribute('data-pin-side', side);

        table.querySelectorAll('.tech-col-pinned').forEach(function (cell) {

            const key = cell.dataset.key || '__select__';

            cell.style[otherSide] = '';
            cell.style[side] = (offsets[key] || 0) + 'px';

            cell.classList.toggle('tech-col-pinned-edge', key === lastKey);
        });
    }

    let pinnedResizeTimer = 0;

    window.addEventListener('resize', function () {

        if (pinnedResizeTimer) clearTimeout(pinnedResizeTimer);

        pinnedResizeTimer = setTimeout(applyPinnedOffsets, 100);
    });

    function buildInfiniteFooterHtml() {

        const loaded = currentRows.length;
        const total = Number(
            lastResult.grouped ? lastResult.rowTotal : lastResult.total) || 0;
        const totalCount = Math.max(total, loaded);

        let groupedNote = '';

        if (lastResult.grouped) {

            const loadedGroups = Math.min(
                lastResult.total, state.page * state.pageSize);

            groupedNote =
                ` · ${loadedGroups.toLocaleString()} گروه`;
        }

        let statusText =
            `نمایش ${loaded.toLocaleString()} از ${totalCount.toLocaleString()} رکورد` +
            groupedNote;

        if (loadingMore) statusText += ' · در حال بارگذاری…';

        if (infiniteDone) {

            statusText +=
                ' <span class="tech-grid-infinite-end">' +
                'همه‌ی رکوردها نمایش داده شد.</span>';
        }

        const button = infiniteDone
            ? ''
            : '<button type="button" ' +
                'class="btn btn-sm btn-outline-primary tech-infinite-more"' +
                (loadingMore ? ' disabled' : '') + '>' +
                (loadingMore
                    ? '<span class="spinner-border spinner-border-sm me-1" ' +
                        'aria-hidden="true"></span>در حال بارگذاری…'
                    : 'بارگذاری بیشتر') +
                '</button>';

        return '<div class="tech-grid-infinite" tabindex="-1">' +
            '<span class="tech-grid-infinite-count" role="status" ' +
            'aria-live="polite">' +
            statusText +
            '</span>' +
            button +
            '</div>';
    }

    function updateInfiniteFooter() {

        const host = el.querySelector('.tech-grid-infinite');

        if (!host) return;

        const hadFocus = host.contains(document.activeElement);

        const template = document.createElement('template');

        template.innerHTML = buildInfiniteFooterHtml();

        const next = template.content.firstElementChild;

        host.replaceWith(next);

        if (hadFocus) {

            next.setAttribute('tabindex', '-1');

            const button = next.querySelector('.tech-infinite-more');

            if (button && !button.disabled) button.focus({ preventScroll: true });
            else next.focus({ preventScroll: true });
        }
    }

    function buildPagerHtml() {

        if (infiniteEnabled) return buildInfiniteFooterHtml();

        if (!pageableEnabled) return '';

        const total = lastResult.total;
        const grouped = lastResult.grouped;
        const page = state.page;
        const pageSize = state.pageSize;

        const totalPages = Math.max(Math.ceil(total / pageSize), 1);

        let html = '<div class="tech-grid-pager">';

        html += '<div class="tech-grid-pager-left">';

        if (pageSizeSelectorEnabled) {

            const sizes = pageSizeOptions.slice();

            if (sizes.indexOf(pageSize) < 0) {
                sizes.push(pageSize);
                sizes.sort(function (a, b) { return a - b; });
            }

            html +=
                '<div class="d-flex align-items-center gap-2 tech-page-size">' +
                `<label class="small mb-0">${grouped ? 'تعداد گروه:' : 'تعداد سطر:'}</label>` +
                '<select class="form-select form-select-sm tech-page-size-select">' +
                sizes.map(function (size) {
                    return `<option value="${size}" ${size === pageSize ? 'selected' : ''}>${size}</option>`;
                }).join('') +
                '</select></div>';
        }

        html += '</div>';

        const totalLabel = grouped
            ? `(${total.toLocaleString()} گروه، ${lastResult.rowTotal.toLocaleString()} رکورد)`
            : `(${total.toLocaleString()} رکورد)`;

        html += '<div class="tech-grid-pager-center">' +
            `صفحه ${page} از ${totalPages}` +
            `<span class="tech-grid-total">${totalLabel}</span>` +
            '</div>';

        html += '<div class="tech-grid-pager-right">';

        html += '<button type="button" class="tech-page-btn tech-page-first" ' +
            'aria-label="اولین صفحه" title="اولین صفحه" ' +
            (page === 1 ? 'disabled' : '') + '>' +
            '<i class="bi bi-chevron-double-right"></i></button>';

        html += '<button type="button" class="tech-page-btn tech-page-prev" ' +
            'aria-label="صفحه قبل" title="صفحه قبل" ' +
            (page === 1 ? 'disabled' : '') + '>' +
            '<i class="bi bi-chevron-right"></i></button>';

        html += '<div class="tech-page-numbers">';

        const maxButtons = 5;

        let start = Math.max(1, page - 2);
        let end = Math.min(totalPages, start + maxButtons - 1);

        if (end - start < maxButtons - 1) {
            start = Math.max(1, end - maxButtons + 1);
        }

        if (start > 1) {

            html += '<button type="button" class="tech-page-btn tech-page-number" ' +
                'data-page="1">1</button>';

            if (start > 2) {
                html += '<span class="tech-page-ellipsis">…</span>';
            }
        }

        for (let i = start; i <= end; i++) {

            html += '<button type="button" ' +
                `class="tech-page-btn tech-page-number ${i === page ? 'active' : ''}" ` +
                `${i === page ? 'aria-current="page"' : ''} ` +
                `data-page="${i}">${i}</button>`;
        }

        if (end < totalPages) {

            if (end < totalPages - 1) {
                html += '<span class="tech-page-ellipsis">…</span>';
            }

            html += '<button type="button" class="tech-page-btn tech-page-number" ' +
                `data-page="${totalPages}">${totalPages}</button>`;
        }

        html += '</div>';

        html += '<button type="button" class="tech-page-btn tech-page-next" ' +
            'aria-label="صفحه بعد" title="صفحه بعد" ' +
            (page === totalPages ? 'disabled' : '') + '>' +
            '<i class="bi bi-chevron-left"></i></button>';

        html += '<button type="button" class="tech-page-btn tech-page-last" ' +
            'aria-label="آخرین صفحه" title="آخرین صفحه" ' +
            (page === totalPages ? 'disabled' : '') + '>' +
            '<i class="bi bi-chevron-double-left"></i></button>';

        return html + '</div></div>';
    }

    function render() {

        const bodyStyle = height > 0
            ? `style="max-height:${height}px; overflow-y:auto;"`
            : '';

        const tableClass = 'table table-striped table-hover mb-0' +
            (resizableEnabled ? ' tech-grid-table-fixed' : '');

        const previousBody = el.querySelector('.tech-grid-body');

        const scrollTop = previousBody ? previousBody.scrollTop : 0;
        const scrollLeft = previousBody ? previousBody.scrollLeft : 0;

        const columnCount = visibleColumns().length +
            (multiSelectEnabled ? 1 : 0);

        const activeElement = document.activeElement;

        const focusState = activeElement &&
            activeElement.matches &&
            activeElement.matches('td[data-key]') &&
            el.contains(activeElement)
            ? {
                rowIndex: parseInt(
                    activeElement.parentElement.dataset.rowIndex, 10),
                key: activeElement.dataset.key
            }
            : null;

        const infiniteFocusState = infiniteEnabled && activeElement &&
            activeElement.closest &&
            activeElement.closest('.tech-grid-infinite') &&
            el.contains(activeElement);

        el.innerHTML =
            '<div class="tech-grid-loading" role="status">' +
            '<div class="tech-grid-spinner"></div>' +
            '<span class="visually-hidden">در حال بارگذاری…</span>' +
            '</div>' +
            buildToolbarHtml() +
            buildFilterBarHtml() +
            `<div class="tech-grid-body table-responsive" ${bodyStyle}>` +
            `<table class="${tableClass}" aria-label="${Tech.escapeHtml(gridLabel)}" ` +
            `aria-rowcount="${lastResult.rowTotal}" aria-colcount="${columnCount}">` +
            buildHeaderHtml() +
            buildBodyHtml() +
            buildFooterHtml() +
            '</table>' +
            (infiniteEnabled
                ? '<div class="tech-grid-infinite-sentinel" aria-hidden="true"></div>'
                : '') +
            '</div>' +
            buildPagerHtml();

        bindGridEvents();

        if (multiSelectEnabled) syncSelectionUi();

        applyPinnedOffsets();

        const body = el.querySelector('.tech-grid-body');

        if (body) {

            body.scrollTop = scrollTop;
            body.scrollLeft = scrollLeft;
        }

        if (chatPanel) {

            el.appendChild(chatPanel);

            syncChatPanel();
        }

        if (pendingFocusField) {

            const field = pendingFocusField;

            pendingFocusField = null;

            const header = Array.from(el.querySelectorAll('.tech-sortable'))
                .find(function (item) {
                    return item.dataset.field === field;
                });

            if (header) header.focus();
        }
        else if (focusState) {

            const rows = getNavRows();

            const row = rows.find(function (item) {
                return parseInt(item.dataset.rowIndex, 10) === focusState.rowIndex;
            });

            const cells = getNavCells(row);

            const cell = cells.find(function (item) {
                return item.dataset.key === focusState.key;
            }) || cells[0];

            if (cell) {

                cells.forEach(function (item) {
                    item.setAttribute('tabindex', '-1');
                });

                cell.setAttribute('tabindex', '0');
                cell.focus();
            }
        }

        if (pendingGroupFocusKey !== null) {

            const key = pendingGroupFocusKey;

            pendingGroupFocusKey = null;

            const toggle = Array.from(
                el.querySelectorAll('.tech-grid-group-toggle'))
                .find(function (button) {
                    return button.dataset.groupKey === key;
                });

            if (toggle) toggle.focus();
        }

        if (infiniteFocusState) {

            const footer = el.querySelector('.tech-grid-infinite');

            if (footer) {

                const button = footer.querySelector('.tech-infinite-more');

                if (button && !button.disabled) {
                    button.focus({ preventScroll: true });
                }
                else {
                    footer.focus({ preventScroll: true });
                }
            }
        }

        if (infiniteEnabled) scheduleInfiniteCheck();
    }

    // --------------------------------------------------------
    // Grid events
    // --------------------------------------------------------

    function selectionPayload() {

        const payload = {
            sender: api,
            data: api.selectedData()
        };

        if (multiSelectEnabled) payload.selected = api.selectedRows();

        return payload;
    }

    function syncSelectionUi() {

        const total = currentRows.length;
        const count = state.selectedIndices.size;

        el.querySelectorAll('.tech-grid-row-check').forEach(function (input) {

            const index = parseInt(input.dataset.index, 10);
            const checked = state.selectedIndices.has(index);

            input.checked = checked;

            const row = input.closest('.tech-grid-row');

            if (!row) return;

            const isSelected = checked || index === state.selectedIndex;

            row.classList.toggle('tech-row-selected', isSelected);
            row.setAttribute('aria-selected', isSelected ? 'true' : 'false');
        });

        const selectAll = el.querySelector('.tech-grid-select-all');

        if (selectAll) {

            selectAll.checked = total > 0 && count === total;
            selectAll.indeterminate = count > 0 && count < total;
        }
    }

    function toggleRowSelection(index, checked) {

        if (!multiSelectEnabled) return;

        if (checked) state.selectedIndices.add(index);
        else state.selectedIndices.delete(index);

        syncSelectionUi();

        triggerGridEvent('Change', selectionPayload());
    }

    function selectRow(row) {

        state.selectedIndex = parseInt(row.dataset.rowIndex, 10);

        if (multiSelectEnabled) state.selectedIndices.clear();

        el.querySelectorAll('.tech-grid-row').forEach(function (item) {

            item.classList.remove('tech-row-selected');
            item.setAttribute('aria-selected', 'false');
        });

        row.classList.add('tech-row-selected');
        row.setAttribute('aria-selected', 'true');

        if (multiSelectEnabled) syncSelectionUi();
        else triggerGridEvent('Change', selectionPayload());
    }

    // --------------------------------------------------------
    // Cell keyboard navigation
    // --------------------------------------------------------

    function getNavRows() {

        return Array.from(
            el.querySelectorAll('.tech-grid-body tbody tr[data-row-index]'));
    }

    function getNavCells(row) {

        return row
            ? Array.from(row.querySelectorAll('td[data-key]'))
            : [];
    }

    function clamp(value, min, max) {

        return Math.max(min, Math.min(max, value));
    }

    function focusCell(rowIndex, columnIndex) {

        const rows = getNavRows();

        if (rows.length === 0) return null;

        const row = rows[clamp(rowIndex, 0, rows.length - 1)];

        const cells = getNavCells(row);

        if (cells.length === 0) return null;

        const cell = cells[clamp(columnIndex, 0, cells.length - 1)];

        el.querySelectorAll('td[data-key][tabindex="0"]').forEach(function (item) {
            item.setAttribute('tabindex', '-1');
        });

        cell.setAttribute('tabindex', '0');
        cell.focus();

        return cell;
    }

    function onGridKeydown(ev) {

        if (ev.altKey || ev.metaKey) return;

        const target = ev.target;

        if (!target.closest) return;

        if (target.closest('a, button, input, select, textarea')) return;

        const cell = target.closest('td[data-key]');

        if (!cell) return;

        const row = cell.closest('tr');
        const rows = getNavRows();
        const rowIndex = rows.indexOf(row);
        const cells = getNavCells(row);
        const columnIndex = cells.indexOf(cell);

        if (rowIndex < 0 || columnIndex < 0) return;

        const rtl = getComputedStyle(el).direction === 'rtl';

        let targetRow = rowIndex;
        let targetColumn = columnIndex;

        switch (ev.key) {

            case 'ArrowDown':
                targetRow = rowIndex + 1;
                break;

            case 'ArrowUp':
                targetRow = rowIndex - 1;
                break;

            case 'ArrowRight':
                targetColumn = rtl ? columnIndex - 1 : columnIndex + 1;
                break;

            case 'ArrowLeft':
                targetColumn = rtl ? columnIndex + 1 : columnIndex - 1;
                break;

            case 'Home':
                targetColumn = 0;
                if (ev.ctrlKey) targetRow = 0;
                break;

            case 'End':
                targetColumn = cells.length - 1;
                if (ev.ctrlKey) targetRow = rows.length - 1;
                break;

            case 'PageDown':
                targetRow = rowIndex + 10;
                break;

            case 'PageUp':
                targetRow = rowIndex - 10;
                break;

            case 'Enter':
                triggerGridEvent('RowClick', {
                    sender: api,
                    index: rowIndex,
                    row: currentRows[rowIndex] || null,
                    data: currentRows[rowIndex] || null
                });
                ev.preventDefault();
                return;

            case ' ':
            case 'Spacebar':

                if (!selectableEnabled && !multiSelectEnabled) return;

                if (multiSelectEnabled) {
                    toggleRowSelection(
                        rowIndex, !state.selectedIndices.has(rowIndex));
                }
                else {
                    selectRow(row);
                }

                ev.preventDefault();
                return;

            default:
                return;
        }

        if (ev.ctrlKey && ev.key !== 'Home' && ev.key !== 'End') return;

        targetRow = clamp(targetRow, 0, rows.length - 1);
        targetColumn = clamp(targetColumn, 0, cells.length - 1);

        if (targetRow === rowIndex && targetColumn === columnIndex) return;

        ev.preventDefault();

        focusCell(targetRow, targetColumn);
    }

    function bindGroupToggleButton(button) {

        button.addEventListener('click', function (ev) {

            ev.stopPropagation();

            const key = button.dataset.groupKey;

            const collapsed = !state.collapsedGroups.has(key);

            if (collapsed) state.collapsedGroups.add(key);
            else state.collapsedGroups.delete(key);

            pendingGroupFocusKey = key;

            render();

            triggerGridEvent('GroupToggle', {
                sender: api,
                key: key,
                collapsed: collapsed
            });
        });
    }

    function bindRowElement(row) {

        function rowPayload() {

            const index = parseInt(row.dataset.rowIndex, 10);

            const data = currentRows[index] || null;

            return { sender: api, index: index, row: data, data: data };
        }

        row.addEventListener('click', function (ev) {

            if (ev.target.closest('a, button, input, select, textarea, label')) {
                return;
            }

            if (multiSelectEnabled) {

                const index = parseInt(row.dataset.rowIndex, 10);

                toggleRowSelection(
                    index, !state.selectedIndices.has(index));
            }
            else if (selectableEnabled) selectRow(row);

            triggerGridEvent('RowClick', rowPayload());
        });

        row.addEventListener('dblclick', function (ev) {

            if (ev.target.closest('a, button, input, select, textarea, label')) {
                return;
            }

            triggerGridEvent('RowDoubleClick', rowPayload());
        });
    }

    function bindRowCheckbox(input) {

        input.addEventListener('change', function () {

            toggleRowSelection(
                parseInt(input.dataset.index, 10),
                input.checked);
        });
    }

    function appendRows(rows, baseIndex) {

        const tbody = el.querySelector('.tech-grid-body tbody');

        if (!tbody) {

            render();
            return;
        }

        // Client side grouping needs to re-group every loaded row so a
        // group is not emitted twice across chunk boundaries.
        if (groupItems.length > 0 && !lastResult.grouped) {

            render();
            return;
        }

        const visible = visibleColumns();
        const colspan = Math.max(visible.length, 1) +
            (multiSelectEnabled ? 1 : 0);

        const highlight = chatState.highlight;

        const highlightFields = highlight
            ? (highlight.field
                ? [highlight.field]
                : visible.map(function (column) { return column.field; }))
            : [];

        let html = '';

        if (groupItems.length > 0) {

            html = buildGroupedRows(
                visible, highlight, highlightFields, colspan,
                rows, baseIndex);
        }
        else {

            rows.forEach(function (row, index) {

                html += buildDataRowHtml(
                    row, baseIndex + index, visible,
                    highlight, highlightFields);
            });
        }

        const emptyRow = tbody.querySelector('.tech-grid-empty');

        if (emptyRow && emptyRow.closest('tr')) {
            emptyRow.closest('tr').remove();
        }

        const template = document.createElement('template');

        template.innerHTML = html;

        template.content.querySelectorAll('.tech-grid-group-toggle')
            .forEach(bindGroupToggleButton);

        template.content.querySelectorAll('.tech-grid-row')
            .forEach(bindRowElement);

        if (multiSelectEnabled) {

            template.content.querySelectorAll('.tech-grid-row-check')
                .forEach(bindRowCheckbox);
        }

        tbody.appendChild(template.content);

        if (multiSelectEnabled) syncSelectionUi();

        applyPinnedOffsets();
    }

    function bindGridEvents() {

        // Active filter chips
        el.querySelectorAll('.tech-filter-chip-remove').forEach(function (button) {

            button.addEventListener('click', function () {

                const field = button.dataset.field;

                state.filters = state.filters.filter(function (filter) {
                    return filter.field !== field;
                });

                state.page = 1;

                load();
            });
        });

        const clearAllFilters = el.querySelector('.tech-filter-clear-all');

        if (clearAllFilters) {

            clearAllFilters.addEventListener('click', function () {

                if (state.filters.length === 0) return;

                state.filters = [];
                state.page = 1;

                load();
            });
        }

        // Cell keyboard navigation
        const tbody = el.querySelector('.tech-grid-body tbody');

        if (tbody) tbody.addEventListener('keydown', onGridKeydown);

        if (infiniteEnabled) {

            const body = el.querySelector('.tech-grid-body');

            if (body) {

                body.addEventListener('scroll', function () {

                    if (infiniteNearBottom()) loadMore();

                }, { passive: true });
            }
        }

        // Sorting
        el.querySelectorAll('.tech-sortable').forEach(function (header) {

            header.addEventListener('click', function (ev) {

                const field = header.dataset.field;

                if (!field) return;

                const index = state.sorts.findIndex(function (sort) {
                    return sort.field === field;
                });

                const existing = index >= 0 ? state.sorts[index] : null;

                if (ev.shiftKey) {

                    if (!existing) {
                        state.sorts.push({ field: field, dir: 'asc' });
                    }
                    else if (existing.dir === 'asc') {
                        existing.dir = 'desc';
                    }
                    else {
                        state.sorts.splice(index, 1);
                    }
                }
                else {

                    if (!existing) {
                        state.sorts = [{ field: field, dir: 'asc' }];
                    }
                    else if (existing.dir === 'asc') {
                        state.sorts = [{ field: field, dir: 'desc' }];
                    }
                    else {
                        state.sorts = [];
                    }
                }

                state.page = 1;

                pendingFocusField = ev.detail === 0 ? field : null;

                load();
            });

            header.addEventListener('keydown', function (ev) {

                if (ev.key === 'Enter' || ev.key === ' ') {

                    ev.preventDefault();

                    header.click();
                }
            });
        });

        // Filter popup
        el.querySelectorAll('.tech-filter-btn').forEach(function (button) {

            button.addEventListener('click', function (ev) {

                ev.stopPropagation();

                closeTechPopups(null);

                openFilterPopup(button);
            });
        });

        // Group headers
        el.querySelectorAll('.tech-grid-group-toggle')
            .forEach(bindGroupToggleButton);

        // Rows: selection + row events
        el.querySelectorAll('.tech-grid-row').forEach(bindRowElement);

        // Multi-select checkboxes
        if (multiSelectEnabled) {

            const selectAll = el.querySelector('.tech-grid-select-all');

            if (selectAll) {

                selectAll.addEventListener('change', function () {

                    state.selectedIndices.clear();

                    if (selectAll.checked) {

                        currentRows.forEach(function (_, index) {
                            state.selectedIndices.add(index);
                        });
                    }

                    state.selectedIndex = -1;

                    syncSelectionUi();

                    triggerGridEvent('Change', selectionPayload());
                });
            }

            el.querySelectorAll('.tech-grid-row-check')
                .forEach(bindRowCheckbox);
        }

        // Toolbar
        el.querySelectorAll('.tech-toolbar-btn').forEach(function (button) {

            button.addEventListener('click', function (ev) {

                const action = button.dataset.action;

                if (action === 'refresh') {

                    api.refresh();
                    return;
                }

                if (action === 'groups') {

                    ev.stopPropagation();

                    if (groupPanel.open) closeGroupPanel();
                    else openGroupPanel();

                    return;
                }

                if (action === 'chat') {

                    ev.stopPropagation();

                    toggleChat();
                    return;
                }

                if (action === 'columnchooser') {

                    ev.stopPropagation();

                    closeTechPopups(null);

                    openColumnChooser(button);
                    return;
                }

                if (action === 'export') {

                    ev.stopPropagation();

                    exportCsv(button.dataset.fileName || null);
                    return;
                }

                const handlerName = button.dataset.handler;

                if (!handlerName) return;

                const fn = window[handlerName];

                if (typeof fn === 'function') {

                    fn({
                        sender: api,
                        selected: api.selectedData(),
                        selectedRows: api.selectedRows()
                    });
                }
            });
        });

        // Page size
        const pageSizeSelect = el.querySelector('.tech-page-size-select');

        if (pageSizeSelect) {

            pageSizeSelect.addEventListener('change', function () {

                state.pageSize = parseInt(pageSizeSelect.value, 10);
                state.page = 1;

                try {
                    localStorage.setItem(
                        pageSizeStorageKey,
                        state.pageSize.toString());
                }
                catch {
                    // ignore
                }

                load();
            });
        }

        // Pagination
        const first = el.querySelector('.tech-page-first');
        const prev = el.querySelector('.tech-page-prev');
        const next = el.querySelector('.tech-page-next');
        const last = el.querySelector('.tech-page-last');

        if (first) {
            first.addEventListener('click', function () {
                state.page = 1;
                load();
            });
        }

        if (prev) {
            prev.addEventListener('click', function () {
                if (state.page > 1) {
                    state.page--;
                    load();
                }
            });
        }

        const totalPages = Math.max(
            Math.ceil(lastResult.total / state.pageSize), 1);

        if (next) {
            next.addEventListener('click', function () {
                if (state.page < totalPages) {
                    state.page++;
                    load();
                }
            });
        }

        if (last) {
            last.addEventListener('click', function () {
                state.page = totalPages;
                load();
            });
        }

        el.querySelectorAll('.tech-page-number').forEach(function (button) {

            button.addEventListener('click', function () {

                state.page = parseInt(button.dataset.page, 10);

                load();
            });
        });

        // Column reorder
        if (reorderableEnabled) {

            let draggedKey = null;

            function clearDropState() {

                el.querySelectorAll('.tech-col-drop-target')
                    .forEach(function (th) {
                        th.classList.remove('tech-col-drop-target');
                    });
            }

            function clearDragState() {

                el.querySelectorAll('.tech-col-dragging')
                    .forEach(function (th) {
                        th.classList.remove('tech-col-dragging');
                    });

                clearDropState();
            }

            el.querySelectorAll('.tech-reorder-handle').forEach(function (handle) {

                handle.addEventListener('dragstart', function (ev) {

                    draggedKey = handle.dataset.key;

                    const th = handle.closest('th');

                    if (th) th.classList.add('tech-col-dragging');

                    if (ev.dataTransfer) {

                        ev.dataTransfer.effectAllowed = 'move';
                        ev.dataTransfer.setData('text/plain', draggedKey);
                    }
                });

                handle.addEventListener('dragend', function () {

                    draggedKey = null;

                    clearDragState();
                });
            });

            el.querySelectorAll('th[data-key]').forEach(function (th) {

                th.addEventListener('dragover', function (ev) {

                    if (!draggedKey || th.dataset.key === draggedKey) return;

                    ev.preventDefault();

                    if (ev.dataTransfer) ev.dataTransfer.dropEffect = 'move';

                    clearDropState();

                    th.classList.add('tech-col-drop-target');
                });

                th.addEventListener('dragleave', function () {

                    th.classList.remove('tech-col-drop-target');
                });

                th.addEventListener('drop', function (ev) {

                    ev.preventDefault();

                    const targetKey = th.dataset.key;
                    const sourceKey = draggedKey;

                    draggedKey = null;

                    clearDragState();

                    if (!sourceKey || !targetKey || sourceKey === targetKey) return;

                    const sourceIndex = columns.findIndex(function (column) {
                        return column.key === sourceKey;
                    });

                    const targetIndex = columns.findIndex(function (column) {
                        return column.key === targetKey;
                    });

                    if (sourceIndex < 0 || targetIndex < 0) return;

                    const moved = columns.splice(sourceIndex, 1)[0];

                    columns.splice(targetIndex, 0, moved);

                    saveColumnOrder();

                    render();
                });
            });
        }

        // Column resize
        if (resizableEnabled) {

            el.querySelectorAll('.tech-resize-handle').forEach(function (handle) {

                handle.addEventListener('mousedown', function (ev) {

                    ev.preventDefault();
                    ev.stopPropagation();

                    const key = handle.dataset.key;

                    const th = handle.closest('th');

                    const startX = ev.clientX;
                    const startWidth = th.offsetWidth;

                    document.body.classList.add('tech-col-resizing');

                    function onMove(moveEvent) {

                        const isRtl =
                            getComputedStyle(el).direction === 'rtl';

                        const delta = moveEvent.clientX - startX;

                        const width = Math.max(
                            80,
                            isRtl ? startWidth - delta : startWidth + delta);

                        th.style.width = width + 'px';

                        const column = columns.find(function (item) {
                            return item.key === key;
                        });

                        if (column) column.width = width;
                    }

                    function onUp() {

                        document.removeEventListener('mousemove', onMove);
                        document.removeEventListener('mouseup', onUp);

                        document.body.classList.remove('tech-col-resizing');

                        saveColumnWidths();

                        applyPinnedOffsets();
                    }

                    document.addEventListener('mousemove', onMove);
                    document.addEventListener('mouseup', onUp);
                });
            });
        }
    }

    // --------------------------------------------------------
    // Filter popup
    // --------------------------------------------------------

    const filterOperators = [
        { value: 'contains', text: 'شامل' },
        { value: 'notcontains', text: 'شامل نیست' },
        { value: 'eq', text: 'مساوی' },
        { value: 'neq', text: 'نامساوی' },
        { value: 'startswith', text: 'شروع با' },
        { value: 'endswith', text: 'پایان با' },
        { value: 'gt', text: 'بزرگ‌تر از' },
        { value: 'gte', text: 'بزرگ‌تر یا مساوی' },
        { value: 'lt', text: 'کوچک‌تر از' },
        { value: 'lte', text: 'کوچک‌تر یا مساوی' },
        { value: 'isnull', text: 'خالی است' },
        { value: 'isnotnull', text: 'خالی نیست' },
        { value: 'in', text: 'یکی از (با , جدا کنید)' },
        { value: 'notin', text: 'هیچ‌کدام (با , جدا کنید)' }
    ];

    function isNullaryOperator(operator) {
        return operator === 'isnull' || operator === 'isnotnull';
    }

    function operatorsForColumn(column) {

        const type = column && column.dataType;

        if (!type) return filterOperators;

        if (type === 'string') {

            const stringOperators = [
                'contains', 'notcontains', 'startswith', 'endswith',
                'eq', 'neq', 'isnull', 'isnotnull', 'in', 'notin'
            ];

            return filterOperators.filter(function (item) {
                return stringOperators.indexOf(item.value) >= 0;
            });
        }

        const allowed = ['eq', 'neq', 'isnull', 'isnotnull', 'in', 'notin'];

        if (type === 'boolean') {
            return filterOperators.filter(function (item) {
                return ['eq', 'neq', 'isnull', 'isnotnull']
                    .indexOf(item.value) >= 0;
            });
        }

        if (type === 'number' || type === 'date' || type === 'time') {
            allowed.push('gt', 'gte', 'lt', 'lte');
        }

        return filterOperators.filter(function (item) {
            return allowed.indexOf(item.value) >= 0;
        });
    }

    function defaultOperatorFor(column) {

        return column && column.dataType && column.dataType !== 'string'
            ? 'eq'
            : 'contains';
    }

    function openFilterPopup(anchor) {

        const field = anchor.dataset.field;

        const column = columns.find(function (item) {
            return item.field === field;
        });

        const operators = operatorsForColumn(column);

        const current = findFilter(field);

        const currentOperator = current ? current.operator : null;

        const operator = operators.some(function (item) {
            return item.value === currentOperator;
        })
            ? currentOperator
            : defaultOperatorFor(column);

        const value = current ? (current.value ?? '') : '';

        const isBooleanColumn = column && column.dataType === 'boolean';

        const inputHtml = isBooleanColumn
            ? '<select class="form-select form-select-sm tech-filter-input">' +
              '<option value="">—</option>' +
              `<option value="true" ${String(value) === 'true' ? 'selected' : ''}>بله</option>` +
              `<option value="false" ${String(value) === 'false' ? 'selected' : ''}>خیر</option>` +
              '</select>'
            : '<input type="text" class="form-control form-control-sm tech-filter-input" ' +
              `value="${Tech.escapeHtml(value)}" placeholder="مقدار...">`;

        const popup = document.createElement('div');

        popup.className = 'tech-filter-popup card shadow-sm';

        if (getComputedStyle(el).direction === 'rtl') {
            popup.classList.add('tech-rtl');
        }

        popup.innerHTML =
            '<div class="card-body p-3">' +
            '<div class="mb-2 fw-semibold">فیلتر</div>' +
            '<select class="form-select form-select-sm mb-2 tech-filter-operator">' +
            operators.map(function (item) {
                return `<option value="${item.value}" ${item.value === operator ? 'selected' : ''}>${item.text}</option>`;
            }).join('') +
            '</select>' +
            inputHtml +
            '<div class="d-flex justify-content-end gap-2 mt-3">' +
            '<button type="button" class="btn btn-sm btn-outline-secondary tech-filter-clear">پاک کردن</button>' +
            '<button type="button" class="btn btn-sm btn-primary tech-filter-apply">اعمال</button>' +
            '</div></div>';

        document.body.appendChild(popup);

        positionPopup(popup, anchor, 260);

        popup.addEventListener('click', function (ev) {
            ev.stopPropagation();
        });

        const operatorSelect = popup.querySelector('.tech-filter-operator');
        const input = popup.querySelector('.tech-filter-input');

        function syncInputState() {

            const nullary = isNullaryOperator(operatorSelect.value);

            input.disabled = nullary;

            if (nullary) input.value = '';
        }

        syncInputState();

        operatorSelect.addEventListener('change', syncInputState);

        function apply() {

            const op = operatorSelect.value;
            const raw = isNullaryOperator(op) ? '' : input.value.trim();

            const index = state.filters.findIndex(function (item) {
                return item.field === field;
            });

            const hasValue = isNullaryOperator(op) || raw !== '';

            if (!hasValue) {

                if (index >= 0) state.filters.splice(index, 1);
            }
            else if (index >= 0) {

                state.filters[index] = { field: field, operator: op, value: raw };
            }
            else {

                state.filters.push({ field: field, operator: op, value: raw });
            }

            state.page = 1;

            closeTechPopups(null);

            load();
        }

        function clear() {

            state.filters = state.filters.filter(function (item) {
                return item.field !== field;
            });

            state.page = 1;

            closeTechPopups(null);

            load();
        }

        popup.querySelector('.tech-filter-apply')
            .addEventListener('click', apply);

        popup.querySelector('.tech-filter-clear')
            .addEventListener('click', clear);

        input.addEventListener('keydown', function (ev) {

            if (ev.key === 'Enter') {
                ev.preventDefault();
                apply();
            }
        });

        if (!input.disabled) {
            input.focus();

            if (typeof input.select === 'function') input.select();
        }
    }

    // --------------------------------------------------------
    // Column chooser
    // --------------------------------------------------------

    function openColumnChooser(anchor) {

        const popup = document.createElement('div');

        popup.className = 'tech-column-popup card shadow-sm';

        if (getComputedStyle(el).direction === 'rtl') {
            popup.classList.add('tech-rtl');
        }

        let html = '<div class="card-body p-3">' +
            '<div class="fw-semibold mb-2">ستون‌ها</div>';

        columns.forEach(function (column) {

            html += '<div class="form-check mb-1">' +
                `<input class="form-check-input tech-column-toggle" type="checkbox" ` +
                `data-key="${Tech.escapeHtml(column.key)}" ${column.hidden ? '' : 'checked'}>` +
                `<label class="form-check-label">${Tech.escapeHtml(column.title)}</label>` +
                '</div>';
        });

        popup.innerHTML = html + '</div>';

        document.body.appendChild(popup);

        positionPopup(popup, anchor, 220);

        popup.addEventListener('click', function (ev) {
            ev.stopPropagation();
        });

        popup.querySelectorAll('.tech-column-toggle').forEach(function (checkbox) {

            checkbox.addEventListener('change', function () {

                const key = checkbox.dataset.key;

                const column = columns.find(function (item) {
                    return item.key === key;
                });

                if (!column) return;

                if (!checkbox.checked && visibleColumns().length <= 1) {

                    checkbox.checked = true;
                    return;
                }

                column.hidden = !checkbox.checked;

                saveColumnState();

                render();
            });
        });
    }

    // --------------------------------------------------------
    // Chat
    // --------------------------------------------------------

    const chatOperators = [
        'contains', 'notcontains', 'startswith', 'endswith',
        'eq', 'neq', 'gt', 'gte', 'lt', 'lte',
        'in', 'notin', 'isnull', 'isnotnull'
    ];

    const chatPanel = chatOptions ? createChatPanel() : null;

    function createChatPanel() {

        const panel = document.createElement('div');

        panel.className = 'tech-grid-chat';
        panel.setAttribute('role', 'dialog');
        panel.setAttribute('aria-modal', 'false');
        panel.setAttribute('aria-label', chatOptions.title || 'دستیار گرید');

        const maxLength = parseInt(chatOptions.maxLength, 10) || 500;

        panel.innerHTML =
            '<div class="tech-grid-chat-header">' +
            '<div class="tech-grid-chat-heading">' +
            `<i class="bi bi-${Tech.escapeHtml(chatOptions.icon || 'chat-dots')}"></i>` +
            `<span class="tech-grid-chat-title">${Tech.escapeHtml(chatOptions.title || 'دستیار گرید')}</span>` +
            '</div>' +
            '<button type="button" class="tech-grid-chat-close" aria-label="بستن دستیار">' +
            '<i class="bi bi-x-lg"></i></button>' +
            '</div>' +
            '<div class="tech-grid-chat-messages" role="log" ' +
            'aria-live="polite" aria-relevant="additions"></div>' +
            '<div class="tech-grid-chat-footer">' +
            `<textarea class="tech-grid-chat-input" rows="1" maxlength="${maxLength}" ` +
            `placeholder="${Tech.escapeHtml(chatOptions.placeholder || '')}" ` +
            'aria-label="پیام به دستیار"></textarea>' +
            '<button type="button" class="tech-grid-chat-send" aria-label="ارسال">' +
            '<i class="bi bi-send"></i></button>' +
            '</div>';

        panel.querySelector('.tech-grid-chat-close')
            .addEventListener('click', function () {
                setChatOpen(false);
            });

        panel.querySelector('.tech-grid-chat-send')
            .addEventListener('click', function () {
                sendChat(panel.querySelector('.tech-grid-chat-input').value);
            });

        panel.querySelector('.tech-grid-chat-input')
            .addEventListener('keydown', function (ev) {

                if (ev.key === 'Enter' && !ev.shiftKey) {

                    ev.preventDefault();

                    sendChat(this.value);
                }
            });

        panel.querySelector('.tech-grid-chat-messages')
            .addEventListener('click', function (ev) {

                const chip = ev.target.closest('.tech-grid-chat-suggestion');

                if (chip) sendChat(chip.dataset.text || '');
            });

        panel.addEventListener('keydown', function (ev) {

            if (ev.key === 'Escape') {
                setChatOpen(false);
            }
        });

        return panel;
    }

    function setChatOpen(open) {

        if (!chatOptions) return;

        chatState.open = !!open;

        if (chatState.open) chatState.focusInput = true;

        if (chatPanel) el.appendChild(chatPanel);

        syncChatPanel();
    }

    function toggleChat() {

        setChatOpen(!chatState.open);
    }

    function syncChatPanel() {

        if (!chatPanel) return;

        chatPanel.classList.toggle('show', chatState.open);

        el.querySelectorAll('.tech-grid-chat-toggle').forEach(function (button) {
            button.setAttribute('aria-expanded', chatState.open ? 'true' : 'false');
        });

        if (chatState.open && chatState.focusInput) {

            chatState.focusInput = false;

            const input = chatPanel.querySelector('.tech-grid-chat-input');

            if (input) input.focus();
        }

        const send = chatPanel.querySelector('.tech-grid-chat-send');

        if (send) send.disabled = chatState.busy;
    }

    function chatMessageHtml(message) {

        const role = message.role === 'user' || message.role === 'assistant'
            ? message.role
            : 'error';

        return '<div class="tech-grid-chat-message tech-grid-chat-message-' + role + '">' +
            '<div class="tech-grid-chat-bubble">' + Tech.escapeHtml(message.content) + '</div>' +
            '</div>';
    }

    function renderChatMessages() {

        if (!chatPanel) return;

        const container = chatPanel.querySelector('.tech-grid-chat-messages');

        if (!container) return;

        let html = '';

        if (chatState.messages.length === 0) {

            if (chatOptions.welcome) {

                html += '<div class="tech-grid-chat-welcome">' +
                    Tech.escapeHtml(chatOptions.welcome) +
                    '</div>';
            }

            if (Array.isArray(chatOptions.suggestions) &&
                chatOptions.suggestions.length > 0) {

                html += '<div class="tech-grid-chat-suggestions">' +
                    chatOptions.suggestions.map(function (suggestion) {
                        return '<button type="button" class="tech-grid-chat-suggestion" ' +
                            `data-text="${Tech.escapeHtml(suggestion)}">` +
                            Tech.escapeHtml(suggestion) +
                            '</button>';
                    }).join('') +
                    '</div>';
            }
        }

        chatState.messages.forEach(function (message) {
            html += chatMessageHtml(message);
        });

        if (chatState.busy) {

            html += '<div class="tech-grid-chat-message tech-grid-chat-message-assistant">' +
                '<div class="tech-grid-chat-bubble tech-grid-chat-typing">' +
                '<span></span><span></span><span></span>' +
                '</div></div>';
        }

        container.innerHTML = html;
        container.scrollTop = container.scrollHeight;
    }

    function buildChatGrid() {

        const snapshot = api.state();

        return {
            id: el.id,
            total: lastResult.rowTotal,
            columns: columns.map(function (column) {

                return {
                    field: column.field,
                    title: column.title,
                    dataType: column.dataType,
                    sortable: !!column.field && sortableEnabled && column.sortable,
                    filterable: !!column.field && filterableEnabled && column.filterable,
                    hidden: !!column.hidden
                };
            }),
            state: {
                page: snapshot.page,
                pageSize: snapshot.pageSize,
                sorts: snapshot.sorts,
                filter: {
                    logic: 'and',
                    filters: snapshot.filters
                }
            }
        };
    }

    async function postChat(body) {

        const headers = {
            'Accept': 'application/json',
            'Content-Type': 'application/json; charset=UTF-8'
        };

        const token = Tech.getAntiforgeryToken(el);

        if (token) headers['RequestVerificationToken'] = token;

        const response = await fetch(chatOptions.url, {
            method: 'POST',
            headers: headers,
            credentials: 'same-origin',
            body: JSON.stringify(body)
        });

        if (!response.ok) {
            throw new Error(`HTTP ${response.status} ${response.statusText}`.trim());
        }

        return response.json();
    }

    async function sendChat(raw) {

        const message = String(raw || '').trim();

        if (!chatOptions || !message || chatState.busy) return;

        const history = chatState.messages
            .filter(function (item) {
                return item.role === 'user' || item.role === 'assistant';
            })
            .slice(-12)
            .map(function (item) {
                return { role: item.role, content: item.content };
            });

        const input = chatPanel.querySelector('.tech-grid-chat-input');

        if (input) input.value = '';

        chatState.messages.push({ role: 'user', content: message });
        chatState.busy = true;
        chatState.open = true;

        renderChatMessages();
        syncChatPanel();

        try {

            const data = await postChat({
                message: message,
                history: history,
                grid: buildChatGrid()
            });

            const reply = data && (data.reply ?? data.Reply);

            const actions = data && (data.actions ?? data.Actions);

            chatState.messages.push({
                role: 'assistant',
                content: typeof reply === 'string' && reply.trim()
                    ? reply.trim()
                    : 'انجام شد.'
            });

            await applyChatActions(actions);
        }
        catch (err) {

            console.error('TechUI Grid chat error:', err);

            chatState.messages.push({
                role: 'error',
                content: 'ارتباط با دستیار برقرار نشد. دوباره تلاش کنید.'
            });

            triggerGridEvent('ChatError', { sender: api, error: err });
        }
        finally {

            chatState.busy = false;
            chatState.focusInput = true;

            renderChatMessages();
            syncChatPanel();
        }
    }

    function chatColumn(field) {

        if (!field) return null;

        return columns.find(function (column) {
            return column.field === field;
        }) || null;
    }

    function sameCellValue(left, right) {

        return String(left ?? '').trim().toLowerCase() ===
            String(right ?? '').trim().toLowerCase();
    }

    function ensureVisibleColumn() {

        if (visibleColumns().length > 0) return;

        if (columns.length > 0) columns[0].hidden = false;
    }

    async function applyChatActions(actions) {

        if (!Array.isArray(actions) || actions.length === 0) return;

        let needsLoad = false;
        let needsRender = false;
        let selection = null;
        let highlight = null;
        let clearHighlightRequested = false;

        actions.slice(0, 20).forEach(function (action) {

            if (!action || typeof action !== 'object') return;

            const type = String(action.type ?? action.Type ?? '').toLowerCase();

            switch (type) {

                case 'filter': {

                    const field = action.field ?? action.Field;
                    const column = chatColumn(field);

                    if (!column || !filterableEnabled || !column.filterable) return;

                    let operator = String(action.operator ?? action.Operator ?? '').toLowerCase();

                    if (chatOperators.indexOf(operator) < 0) {
                        operator = defaultOperatorFor(column);
                    }

                    const value = action.value ?? action.Value;
                    const nullary = operator === 'isnull' || operator === 'isnotnull';

                    if (!nullary && (value === null || value === undefined || String(value) === '')) {
                        return;
                    }

                    const filter = {
                        field: column.field,
                        operator: operator,
                        value: nullary ? '' : String(value)
                    };

                    const index = state.filters.findIndex(function (item) {
                        return item.field === column.field;
                    });

                    if (index >= 0) state.filters[index] = filter;
                    else state.filters.push(filter);

                    state.page = 1;
                    needsLoad = true;
                    return;
                }

                case 'clearfilters':

                    state.filters = [];
                    state.page = 1;
                    needsLoad = true;
                    return;

                case 'sort': {

                    const column = chatColumn(action.field ?? action.Field);

                    if (!column || !sortableEnabled || !column.sortable) return;

                    const dir = String(action.dir ?? action.Dir ?? '').toLowerCase();

                    state.sorts = [{ field: column.field, dir: dir === 'desc' ? 'desc' : 'asc' }];
                    state.page = 1;
                    needsLoad = true;
                    return;
                }

                case 'clearsorts':

                    state.sorts = [];
                    state.page = 1;
                    needsLoad = true;
                    return;

                case 'page': {

                    let page = parseInt(action.page ?? action.Page, 10);

                    if (!page || page < 1) return;

                    const totalPages = Math.max(
                        Math.ceil(lastResult.total / state.pageSize), 1);

                    page = Math.min(page, totalPages);

                    if (page !== state.page) {

                        state.page = page;
                        needsLoad = true;
                    }

                    return;
                }

                case 'pagesize': {

                    const raw = parseInt(action.pageSize ?? action.PageSize, 10);

                    if (!raw || raw < 1) return;

                    const size = Math.min(raw, 1000);

                    if (size !== state.pageSize) {

                        state.pageSize = size;
                        state.page = 1;

                        try {
                            localStorage.setItem(pageSizeStorageKey, size.toString());
                        }
                        catch {
                            // ignore
                        }

                        needsLoad = true;
                    }

                    return;
                }

                case 'hidecolumns':
                case 'showcolumns': {

                    const fields = action.fields ?? action.Fields;

                    if (!Array.isArray(fields)) return;

                    const hidden = type === 'hidecolumns';

                    fields.forEach(function (field) {

                        const column = chatColumn(field);

                        if (column) column.hidden = hidden;
                    });

                    ensureVisibleColumn();
                    saveColumnState();

                    needsRender = true;
                    return;
                }

                case 'showallcolumns':

                    columns.forEach(function (column) {
                        column.hidden = false;
                    });

                    saveColumnState();

                    needsRender = true;
                    return;

                case 'select': {

                    const field = action.field ?? action.Field;
                    const value = action.value ?? action.Value;

                    if (!field || value === null || value === undefined) return;

                    selection = { field: field, value: String(value) };
                    return;
                }

                case 'highlight': {

                    const field = action.field ?? action.Field;
                    const value = action.value ?? action.Value;

                    if (value === null || value === undefined || String(value) === '') return;

                    const column = field ? chatColumn(field) : null;

                    highlight = {
                        field: column ? column.field : null,
                        value: String(value)
                    };

                    return;
                }

                case 'clearhighlight':

                    clearHighlightRequested = true;
                    return;

                case 'refresh':

                    needsLoad = true;
                    return;
            }
        });

        if (needsLoad) await load();
        else if (needsRender) render();

        let changed = false;

        if (clearHighlightRequested) {

            chatState.highlight = null;
            changed = true;
        }

        if (highlight) {

            chatState.highlight = highlight;
            changed = true;
        }

        if (selection) {

            const index = currentRows.findIndex(function (row) {

                return sameCellValue(getValue(row, selection.field), selection.value);
            });

            if (index >= 0) {

                state.selectedIndex = index;
                changed = true;
            }
        }

        if (changed) render();

        if (selection && state.selectedIndex >= 0) {

            triggerGridEvent('Change', selectionPayload());
        }
    }

    renderChatMessages();

    // --------------------------------------------------------
    // CSV export
    // --------------------------------------------------------

    function csvEscape(value) {

        const text = String(value ?? '')
            .replace(/\r?\n/g, ' ')
            .trim();

        return /[",]/.test(text)
            ? '"' + text.replace(/"/g, '""') + '"'
            : text;
    }

    function exportCsv(fileName) {

        if (currentRows.length === 0) {

            Tech.notify('داده‌ای برای خروجی وجود ندارد.', {
                type: 'warning',
                title: 'خروجی CSV'
            });

            return;
        }

        const visible = visibleColumns();

        const lines = [
            visible.map(function (column) {
                return csvEscape(column.title);
            }).join(',')
        ];

        el.querySelectorAll('.tech-grid-body tbody tr[data-row-index]')
            .forEach(function (row) {

                const cells = row.querySelectorAll('td[data-key]');

                lines.push(Array.from(cells).map(function (cell) {
                    return csvEscape(cell.textContent);
                }).join(','));
            });

        let name = fileName ||
            (el.id + '-' + new Date().toISOString().slice(0, 10));

        if (!/\.csv$/i.test(name)) name += '.csv';

        name = name.replace(/[\\/:*?"<>|]+/g, '-');

        const blob = new Blob(['\uFEFF' + lines.join('\r\n')], {
            type: 'text/csv;charset=utf-8;'
        });

        const url = URL.createObjectURL(blob);

        const link = document.createElement('a');

        link.href = url;
        link.download = name;

        document.body.appendChild(link);

        link.click();

        document.body.removeChild(link);

        setTimeout(function () {
            URL.revokeObjectURL(url);
        }, 0);
    }

    // --------------------------------------------------------
    // Public API
    // --------------------------------------------------------

    const api = {

        refresh: async function () {

            if (infiniteEnabled) {

                state.page = 1;
                currentRows = [];
            }

            await load();

            return api;
        },

        loadMore: async function () {

            await loadMore();

            return api;
        },

        hasMore: function () {

            return infiniteEnabled && !infiniteDone && !loadingMore;
        },

        selectedData: function () {

            if (multiSelectEnabled && state.selectedIndices.size > 0) {

                const index = Array.from(state.selectedIndices)[0];

                return currentRows[index] || null;
            }

            return state.selectedIndex >= 0
                ? (currentRows[state.selectedIndex] || null)
                : null;
        },

        selectedRows: function () {

            return Array.from(state.selectedIndices)
                .sort(function (a, b) { return a - b; })
                .map(function (index) { return currentRows[index]; })
                .filter(Boolean);
        },

        getData: function () {

            return currentRows.slice();
        },

        exportCsv: function (fileName) {

            exportCsv(fileName);

            return api;
        },

        clearSelection: function () {

            state.selectedIndex = -1;
            state.selectedIndices.clear();

            el.querySelectorAll('.tech-grid-row').forEach(function (row) {

                row.classList.remove('tech-row-selected');

                if (selectableEnabled) {
                    row.setAttribute('aria-selected', 'false');
                }
            });

            if (multiSelectEnabled) syncSelectionUi();

            return api;
        },

        setFilter: function (field, value, operator) {

            const column = columns.find(function (item) {
                return item.field === field;
            });

            const resolvedOperator = operator || defaultOperatorFor(column);

            const index = state.filters.findIndex(function (item) {
                return item.field === field;
            });

            if (value === null || value === undefined || value === '') {

                if (index >= 0) state.filters.splice(index, 1);
            }
            else if (index >= 0) {

                state.filters[index] = {
                    field: field,
                    operator: resolvedOperator,
                    value: value
                };
            }
            else {

                state.filters.push({
                    field: field,
                    operator: resolvedOperator,
                    value: value
                });
            }

            state.page = 1;

            return api;
        },

        clearFilters: function () {

            state.filters = [];
            state.page = 1;

            return api;
        },

        setSort: function (field, dir) {

            const column = chatColumn(field);

            if (!column) return api;

            state.sorts = [{
                field: column.field,
                dir: dir === 'desc' ? 'desc' : 'asc'
            }];

            state.page = 1;

            return api;
        },

        setSorts: function (sorts) {

            state.sorts = (Array.isArray(sorts) ? sorts : [])
                .map(function (sort) {

                    const column = chatColumn(sort && sort.field);

                    if (!column) return null;

                    return {
                        field: column.field,
                        dir: sort.dir === 'desc' ? 'desc' : 'asc'
                    };
                })
                .filter(Boolean);

            state.page = 1;

            return api;
        },

        clearSorts: function () {

            state.sorts = [];
            state.page = 1;

            return api;
        },

        setGroups: function (groups) {

            applyGroups(groups);

            refreshGroupPanel();

            return api;
        },

        addGroup: function (field, title) {

            if (!field) return api;

            const exists = groupItems.some(function (group) {
                return group.field.toLowerCase() ===
                    String(field).toLowerCase();
            });

            if (exists) return api;

            applyGroups(groupItems.concat([{
                field: field,
                title: title || ''
            }]));

            refreshGroupPanel();

            return api;
        },

        removeGroup: function (field) {

            const normalized = String(field).toLowerCase();

            applyGroups(groupItems.filter(function (group) {
                return group.field.toLowerCase() !== normalized;
            }));

            refreshGroupPanel();

            return api;
        },

        clearGroups: function () {

            applyGroups([]);

            refreshGroupPanel();

            return api;
        },

        getGroups: function () {

            return groupItems.map(function (group) {
                return { field: group.field, title: group.title };
            });
        },

        expandAllGroups: function () {

            if (state.collapsedGroups.size === 0) return api;

            state.collapsedGroups.clear();

            render();

            return api;
        },

        collapseAllGroups: function () {

            if (groupItems.length === 0) return api;

            allGroupKeys().forEach(function (key) {
                state.collapsedGroups.add(key);
            });

            render();

            return api;
        },

        getCollapsedGroups: function () {

            return Array.from(state.collapsedGroups);
        },

        setPage: function (page) {

            const parsed = parseInt(page, 10);

            if (parsed > 0) state.page = parsed;

            return api;
        },

        setPageSize: function (size) {

            const parsed = parseInt(size, 10);

            if (parsed > 0) {

                state.pageSize = Math.min(parsed, 1000);
                state.page = 1;

                try {
                    localStorage.setItem(
                        pageSizeStorageKey,
                        state.pageSize.toString());
                }
                catch {
                    // ignore
                }
            }

            return api;
        },

        setColumnVisible: function (fieldOrKey, visible) {

            const column = columns.find(function (item) {
                return item.field === fieldOrKey || item.key === fieldOrKey;
            });

            if (!column) return api;

            column.hidden = !visible;

            ensureVisibleColumn();

            saveColumnState();

            render();

            return api;
        },

        showAllColumns: function () {

            columns.forEach(function (column) {
                column.hidden = false;
            });

            saveColumnState();

            render();

            return api;
        },

        highlight: function (field, value) {

            if (value === null || value === undefined || value === '') {
                return api.clearHighlight();
            }

            const column = field ? chatColumn(field) : null;

            chatState.highlight = {
                field: column ? column.field : null,
                value: String(value)
            };

            render();

            return api;
        },

        clearHighlight: function () {

            chatState.highlight = null;

            render();

            return api;
        },

        selectBy: function (field, value) {

            const column = chatColumn(field);

            if (!column || value === null || value === undefined) return api;

            const index = currentRows.findIndex(function (row) {
                return sameCellValue(getValue(row, column.field), value);
            });

            if (index < 0) return api;

            state.selectedIndex = index;

            render();

            triggerGridEvent('Change', {
                sender: api,
                data: api.selectedData()
            });

            return api;
        },

        chat: chatOptions ? {

            open: function () {
                setChatOpen(true);
                return api;
            },

            close: function () {
                setChatOpen(false);
                return api;
            },

            toggle: function () {
                toggleChat();
                return api;
            },

            isOpen: function () {
                return chatState.open;
            },

            send: function (message) {
                return sendChat(message);
            },

            messages: function () {

                return chatState.messages.map(function (message) {
                    return { role: message.role, content: message.content };
                });
            }
        } : null,

        state: function () {

            return {
                page: state.page,
                pageSize: state.pageSize,
                sorts: state.sorts.map(function (sort) {
                    return { field: sort.field, dir: sort.dir };
                }),
                filters: state.filters.map(function (filter) {
                    return {
                        field: filter.field,
                        operator: filter.operator,
                        value: filter.value
                    };
                }),
                groups: groupItems.map(function (group) {
                    return { field: group.field, title: group.title };
                }),
                collapsedGroups: Array.from(state.collapsedGroups),
                highlight: chatState.highlight
                    ? {
                        field: chatState.highlight.field,
                        value: chatState.highlight.value
                    }
                    : null
            };
        }
    };

    el._techGrid = api;

    el.techGrid = api;

    Tech.Grid.instances.set(el.id, api);

    if (infiniteEnabled) {

        el.addEventListener('click', function (ev) {

            const button = ev.target.closest('.tech-infinite-more');

            if (!button || button.disabled) return;

            ev.preventDefault();

            loadMore();
        });

        window.addEventListener('scroll', function () {

            if (height === 0 && infiniteNearBottom()) loadMore();

        }, { capture: true, passive: true });
    }

    load();
});

// ============================================================
// DropDownList
// ============================================================

Tech.DropDownList = Tech.DropDownList || {};
Tech.DropDownList.instances = new Map();

Tech.DropDownList.get = function (id) {
    return Tech.DropDownList.instances.get(id);
};

function getProperty(obj, name) {

    if (!obj || !name) return undefined;

    if (obj[name] !== undefined) return obj[name];

    const key = Object.keys(obj)
        .find(function (item) {
            return item.toLowerCase() === name.toLowerCase();
        });

    return key ? obj[key] : undefined;
}

function dropdownText(select, item) {

    return getProperty(item, select.dataset.textField || 'text');
}

function dropdownValue(select, item) {

    const valueField = select.dataset.valueField || 'value';

    const value = getProperty(item, valueField);

    if (value !== undefined) return value;

    if (getProperty(item, 'id') !== undefined) return getProperty(item, 'id');

    return getProperty(item, 'value');
}

function dropdownWrapper(select) {

    return select.closest('.tech-dropdown-wrapper');
}

function getSelectedText(select) {

    const option = select.options[select.selectedIndex];

    return option ? option.textContent : '';
}

function updateDisplay(select) {

    const wrapper = dropdownWrapper(select);

    if (!wrapper) return;

    const textEl = wrapper.querySelector('.tech-dropdown-text');

    if (!textEl) return;

    const option = select.options[select.selectedIndex];

    textEl.textContent = option
        ? option.textContent
        : 'انتخاب کنید';
}

function setDropdownDisabled(select, disabled) {

    select.disabled = disabled;

    const wrapper = dropdownWrapper(select);

    if (wrapper) {
        wrapper.classList.toggle('tech-dropdown-disabled', disabled);
    }
}

function clearOptions(select) {

    Array.from(select.options).forEach(function (option) {

        if (option.value !== '') {
            option.remove();
        }
    });
}

function renderOptions(select, items) {

    const currentValue = select.value;
    const currentText = getSelectedText(select);

    clearOptions(select);

    let matched = false;

    items.forEach(function (item) {

        const value = dropdownValue(select, item);

        if (value === null || value === undefined) return;

        if (String(value) === currentValue) matched = true;

        const option = document.createElement('option');

        option.value = String(value);
        option.textContent = dropdownText(select, item) ?? '';

        select.appendChild(option);
    });

    if (currentValue && !matched) {

        // Keep the current selection visible even if the filtered
        // result no longer contains it.
        const option = document.createElement('option');

        option.value = currentValue;
        option.textContent = currentText || currentValue;

        select.appendChild(option);
    }

    if (currentValue) {
        select.value = currentValue;
    }

    updateDisplay(select);

    const wrapper = dropdownWrapper(select);

    if (wrapper && wrapper.classList.contains('open')) {
        techPositionPopup(wrapper);
    }
}

function appendDataFunctionParams(select, params) {

    const dataFunctionName = select.dataset.dataFunction;

    if (!dataFunctionName) return;

    const fn = window[dataFunctionName];

    if (typeof fn !== 'function') return;

    const extra = fn() || {};

    Object.keys(extra).forEach(function (key) {

        const value = extra[key];

        if (value !== null && value !== undefined && value !== '') {
            params.append(key, value);
        }
    });
}

function requestDropdownItems(select, term) {

    const params = new URLSearchParams();

    if (term !== null && term !== undefined) {
        params.append('term', term);
    }

    appendDataFunctionParams(select, params);

    return Tech.requestJson(select.dataset.url, {
        method: select.dataset.method || 'GET',
        params: params,
        antiforgeryElement: select
    });
}

function filterClient(select, term) {

    term = (term || '').trim().toLowerCase();

    const original = select._techOriginalItems || [];

    const textField = select.dataset.textField || 'text';
    const mode = select.dataset.filterMode || 'contains';

    if (!term) {

        select._techItems = original;
        renderOptions(select, original);
        return;
    }

    const filtered = original.filter(function (item) {

        const text = String(
            getProperty(item, textField) ?? '').toLowerCase();

        if (mode === 'startswith') return text.startsWith(term);
        if (mode === 'endswith') return text.endsWith(term);

        return text.indexOf(term) >= 0;
    });

    select._techItems = filtered;

    renderOptions(select, filtered);
}

async function filterServer(select, term) {

    term = (term || '').trim();

    const minLength = parseInt(select.dataset.minLength || '1', 10);

    if (term.length === 0 || term.length < minLength) {

        select._techItems = select._techOriginalItems || [];
        renderOptions(select, select._techItems);
        return;
    }

    try {

        const items = await requestDropdownItems(select, term);

        select._techItems = items || [];

        renderOptions(select, select._techItems);
    }
    catch (err) {

        console.error('TechUI DropDownList filter error:', err);
    }
}

function fireDropdownValue(select) {

    select.dispatchEvent(new CustomEvent('tech:value', {
        bubbles: true,
        detail: { value: select.value }
    }));
}

function applyDropdownValue(select, value) {

    value = value === null || value === undefined ? '' : String(value);

    const option = Array.from(select.options)
        .find(function (item) {
            return item.value === value;
        });

    select.value = option ? value : select.value;

    updateDisplay(select);

    fireDropdownValue(select);
}

function openDropdown(select) {

    const wrapper = dropdownWrapper(select);

    if (!wrapper ||
        select.disabled ||
        wrapper.classList.contains('open')) {
        return;
    }

    document.querySelectorAll('.tech-dropdown-wrapper.open')
        .forEach(function (other) {

            const otherSelect = other.querySelector('select');

            if (otherSelect && otherSelect !== select) {
                closeDropdown(otherSelect);
            }
        });

    wrapper.classList.add('open');

    const display = wrapper.querySelector('.tech-dropdown-display');

    if (display) display.setAttribute('aria-expanded', 'true');

    const panel = wrapper.querySelector('.tech-dropdown-panel');

    if (panel) {
        techFloatPopup(wrapper, panel, {
            anchor: display || wrapper,
            width: 'anchor'
        });
    }

    Tech.trigger(select, 'Open', { sender: select });

    const search = wrapper.querySelector('.tech-dropdown-search');

    if (search) {
        setTimeout(function () { search.focus(); }, 30);
    }
}

function closeDropdown(select) {

    const wrapper = dropdownWrapper(select);

    if (!wrapper || !wrapper.classList.contains('open')) return;

    techReleasePopup(wrapper);

    wrapper.classList.remove('open');

    const display = wrapper.querySelector('.tech-dropdown-display');

    if (display) display.setAttribute('aria-expanded', 'false');

    Tech.trigger(select, 'Close', { sender: select });
}

function resetCascade(select) {

    select._techItems = [];
    select._techOriginalItems = [];

    clearOptions(select);

    select.value = '';

    setDropdownDisabled(select, true);

    const wrapper = dropdownWrapper(select);

    if (wrapper) {

        techReleasePopup(wrapper);

        wrapper.classList.remove('open');
    }
}

async function loadCascade(select, parentValue) {

    if (!select.dataset.url) return;

    if (parentValue === null ||
        parentValue === undefined ||
        parentValue === '') {

        resetCascade(select);

        return;
    }

    setDropdownDisabled(select, true);

    try {

        const params = new URLSearchParams();

        params.append(
            select.dataset.cascadeParentField || 'parentId',
            parentValue);

        appendDataFunctionParams(select, params);

        const items = await Tech.requestJson(select.dataset.url, {
            method: select.dataset.method || 'GET',
            params: params,
            antiforgeryElement: select
        });

        select._techOriginalItems = items || [];
        select._techItems = items || [];

        select.value = '';

        renderOptions(select, items || []);

        setDropdownDisabled(select, false);

        updateDisplay(select);

        Tech.trigger(select, 'Bound', { sender: select });
    }
    catch (err) {

        console.error('TechUI Cascade error:', err);

        resetCascade(select);
    }
}

function wireCascade(select) {

    const parentId = select.dataset.cascadeFrom;

    if (!parentId) return;

    const parent = document.getElementById(parentId);

    if (!parent) {

        setDropdownDisabled(select, true);
        return;
    }

    function onParentValue() {
        loadCascade(select, parent.value);
    }

    parent.addEventListener('change', onParentValue);
    parent.addEventListener('tech:value', onParentValue);

    if (parent.value) {
        loadCascade(select, parent.value);
    }
    else {
        setDropdownDisabled(select, true);
    }
}

function wireDropdownPopup(select) {

    const wrapper = dropdownWrapper(select);

    if (!wrapper) return;

    const display = wrapper.querySelector('.tech-dropdown-display');
    const panel = wrapper.querySelector('.tech-dropdown-panel');
    const search = wrapper.querySelector('.tech-dropdown-search');

    if (!display || !panel) return;

    display.addEventListener('click', function (ev) {

        ev.stopPropagation();

        if (wrapper.classList.contains('open')) {
            closeDropdown(select);
        }
        else {
            openDropdown(select);
        }
    });

    display.addEventListener('keydown', function (ev) {

        if (ev.key === 'Enter' || ev.key === ' ' || ev.key === 'ArrowDown') {
            ev.preventDefault();
            openDropdown(select);
        }
        else if (ev.key === 'Escape') {
            closeDropdown(select);
        }
    });

    panel.addEventListener('click', function (ev) {
        ev.stopPropagation();
    });

    if (search) {

        let timer;

        search.addEventListener('input', function () {

            clearTimeout(timer);

            timer = setTimeout(function () {

                const term = search.value;

                if (select.dataset.serverFiltering === 'true') {
                    filterServer(select, term);
                }
                else {
                    filterClient(select, term);
                }

            }, 250);
        });

        search.addEventListener('keydown', function (ev) {

            if (ev.key === 'Escape') {
                ev.preventDefault();
                closeDropdown(select);
            }
            else if (ev.key === 'Enter' && select.options.length > 0) {
                ev.preventDefault();
                select.focus();
            }
        });
    }
}

function initStaticOptions(select) {

    const items = [];

    Array.from(select.options).forEach(function (option) {

        if (option.value === '') return;

        items.push({
            value: option.value,
            text: option.textContent
        });
    });

    select._techOriginalItems = items;
    select._techItems = items;
}

function createDropdownApi(select) {

    return {

        value: function () {
            return select.value;
        },

        text: function () {
            return select.options[select.selectedIndex]
                ? select.options[select.selectedIndex].textContent
                : null;
        },

        dataItem: function () {

            const items = select._techItems || [];

            const valueField = select.dataset.valueField || 'value';

            return items.find(function (item) {
                return String(dropdownValue(select, item)) === select.value;
            }) || null;
        },

        setValue: function (value) {

            applyDropdownValue(select, value);

            return this;
        },

        reload: async function () {

            if (!select.dataset.url) return this;

            try {

                const items = await requestDropdownItems(select, null);

                select._techOriginalItems = items || [];
                select._techItems = items || [];

                renderOptions(select, items || []);

                Tech.trigger(select, 'Bound', { sender: select });
            }
            catch (err) {
                console.error('TechUI DropDownList reload error:', err);
            }

            return this;
        }
    };
}

async function initDropdown(select) {

    if (select._techInit) return;

    select._techInit = true;

    const api = createDropdownApi(select);

    select.techDropdown = api;

    Tech.DropDownList.instances.set(select.id, api);

    // Synchronous wiring first so cascade parents that finish loading
    // later still reach this dropdown.
    wireCascade(select);

    if (select.closest('.tech-dropdown-filterable')) {
        wireDropdownPopup(select);
    }
    else {

        select.addEventListener('focus', function () {
            Tech.trigger(select, 'Open', { sender: select });
        });

        select.addEventListener('blur', function () {
            Tech.trigger(select, 'Close', { sender: select });
        });
    }

    select.addEventListener('change', function () {

        updateDisplay(select);

        closeDropdown(select);

        Tech.trigger(select, 'Change', {
            sender: select,
            value: select.value
        });
    });

    if (!select.dataset.url) {

        initStaticOptions(select);

        applyDropdownValue(select, select.dataset.value);

        Tech.trigger(select, 'Bound', { sender: select });

        return;
    }

    // Cascade children are populated only through their parent.
    if (select.dataset.cascadeFrom) return;

    try {

        const items = await requestDropdownItems(select, null);

        select._techOriginalItems = items || [];
        select._techItems = items || [];

        renderOptions(select, items || []);

        applyDropdownValue(select, select.dataset.value);

        Tech.trigger(select, 'Bound', { sender: select });
    }
    catch (err) {

        console.error('TechUI DropDownList error:', err);

        Tech.trigger(select, 'Error', { sender: select, error: err });
    }
}

Tech.DropDownList.value = function (id) {

    const select = document.getElementById(id);

    return select ? select.value : null;
};

Tech.DropDownList.text = function (id) {

    const select = document.getElementById(id);

    return select && select.options[select.selectedIndex]
        ? select.options[select.selectedIndex].textContent
        : null;
};

Tech.DropDownList.dataItem = function (id) {

    const select = document.getElementById(id);

    if (!select || !select._techItems) return null;

    const valueField = select.dataset.valueField || 'value';

    return select._techItems.find(function (item) {
        return String(getProperty(item, valueField)) === select.value;
    }) || null;
};

Tech.DropDownList.setValue = function (id, value) {

    const select = document.getElementById(id);

    if (select) applyDropdownValue(select, value);
};

Tech.register('dropdownlist', initDropdown);

// ------------------------------------------------------------
// Close open dropdown panels when clicking outside
// ------------------------------------------------------------

document.addEventListener('click', function (ev) {

    document.querySelectorAll('.tech-dropdown-wrapper.open')
        .forEach(function (wrapper) {

            if (wrapper.contains(ev.target)) return;

            const select = wrapper.querySelector('select');

            if (select) closeDropdown(select);
        });

    document.querySelectorAll('.tech-datepicker.open')
        .forEach(function (root) {

            if (!root.contains(ev.target)) techCloseDatePicker(root);
        });

    document.querySelectorAll('.tech-daterange.open')
        .forEach(function (root) {

            if (!root.contains(ev.target)) techCloseDateRange(root);
        });

    document.querySelectorAll('.tech-timepicker.open')
        .forEach(function (root) {

            if (!root.contains(ev.target)) techCloseTimePicker(root);
        });
});

// ------------------------------------------------------------
// Close popups when a Bootstrap modal / offcanvas hides,
// otherwise fixed panels would linger over the page.
// ------------------------------------------------------------

function techClosePopupsWithin(container) {

    if (!container || !container.querySelectorAll) return;

    container.querySelectorAll('.tech-dropdown-wrapper.open')
        .forEach(function (wrapper) {

            const select = wrapper.querySelector('select');

            if (select) closeDropdown(select);
        });

    container.querySelectorAll('.tech-datepicker.open')
        .forEach(techCloseDatePicker);

    container.querySelectorAll('.tech-daterange.open')
        .forEach(techCloseDateRange);

    container.querySelectorAll('.tech-timepicker.open')
        .forEach(techCloseTimePicker);

    container.querySelectorAll('.tech-multiselect.open')
        .forEach(function (root) {

            if (root._techMultiSelect) root._techMultiSelect.close();
        });

    container.querySelectorAll('.tech-droptree.open')
        .forEach(function (root) {

            if (root._techDropDownTree) root._techDropDownTree.close();
        });
}

function techHandleOverlayHide(ev) {

    techClosePopupsWithin(ev.target);

    closeTechPopups(null);
}

// Close on hide so panels do not linger during the modal fade,
// and again on hidden for modals removed before the transition.
document.addEventListener('hide.bs.modal', techHandleOverlayHide);
document.addEventListener('hidden.bs.modal', techHandleOverlayHide);

document.addEventListener('hide.bs.offcanvas', techHandleOverlayHide);
document.addEventListener('hidden.bs.offcanvas', techHandleOverlayHide);

// ============================================================
// Inputs (textbox, textarea, numeric, masked)
// ============================================================

function techNormalizeDigits(value) {

    if (value === null || value === undefined) return '';

    return String(value)
        .replace(/[\u06F0-\u06F9]/g, function (c) {
            return String.fromCharCode(c.charCodeAt(0) - 0x06F0 + 48);
        })
        .replace(/[\u0660-\u0669]/g, function (c) {
            return String.fromCharCode(c.charCodeAt(0) - 0x0660 + 48);
        });
}

function techInputSyncCounter(root, input, counter) {

    root.classList.toggle('has-value', !!input.value);

    if (!counter) return;

    const max = input.getAttribute('maxlength');

    counter.textContent = max
        ? input.value.length + '/' + max
        : String(input.value.length);
}

function initTextbox(root) {

    const input = root.querySelector('input.tech-input-control');

    if (!input || input._techTextbox) return;

    input._techTextbox = true;

    const clear = root.querySelector('.tech-input-clear');
    const counter = root.querySelector('.tech-input-counter');

    function sync() {
        techInputSyncCounter(root, input, counter);
    }

    input.addEventListener('input', sync);

    input.addEventListener('change', function () {
        Tech.trigger(root, 'Change', {
            sender: input,
            value: input.value
        });
    });

    if (clear) {

        clear.addEventListener('click', function () {

            if (input.value === '') return;

            input.value = '';

            sync();

            input.focus();

            Tech.trigger(root, 'Change', {
                sender: input,
                value: ''
            });
        });
    }

    sync();
}

function initTextarea(root) {

    const area = root.querySelector('textarea');

    if (!area || area._techTextarea) return;

    area._techTextarea = true;

    const counter = root.querySelector('.tech-input-counter');
    const autoGrow = root.dataset.autoGrow === 'true';

    function grow() {

        if (!autoGrow || area.offsetWidth === 0) return;

        area.style.height = 'auto';
        area.style.height = area.scrollHeight + 'px';
    }

    function sync() {

        techInputSyncCounter(root, area, counter);

        grow();
    }

    area.addEventListener('input', sync);

    area.addEventListener('change', function () {
        Tech.trigger(root, 'Change', {
            sender: area,
            value: area.value
        });
    });

    sync();

    requestAnimationFrame(grow);
}

function techCreateNumberFormat(culture, grouping, decimals) {

    const settings = {
        useGrouping: !!grouping,
        minimumFractionDigits: 0,
        maximumFractionDigits: decimals
    };

    try {
        return new Intl.NumberFormat(culture, settings);
    }
    catch {
        return new Intl.NumberFormat('en-US', settings);
    }
}

function techNumberSeparators(format) {

    const separators = { group: ',', decimal: '.' };

    format.formatToParts(12345.6).forEach(function (part) {

        if (part.type === 'group') separators.group = part.value;
        if (part.type === 'decimal') separators.decimal = part.value;
    });

    return separators;
}

function techParseNumber(text, separators) {

    let value = techNormalizeDigits(text).trim();

    if (!value) return null;

    value = value.replace(/[\s\u00A0\u202F]/g, '');

    if (separators.group) {
        value = value.split(separators.group).join('');
    }

    if (separators.decimal) {
        value = value.split(separators.decimal).join('.');
    }

    value = value.replace(/[٬,]/g, '');

    const number = Number(value);

    return Number.isFinite(number) ? number : null;
}

function techRoundNumber(value, decimals) {

    const factor = Math.pow(10, decimals);

    return Math.round((value + Number.EPSILON) * factor) / factor;
}

function initNumeric(root) {

    const visible = root.querySelector('input.tech-input-control');
    const hidden = root.querySelector('input[type="hidden"]');

    if (!visible || visible._techNumeric) return;

    visible._techNumeric = true;

    const culture = root.dataset.culture || 'fa-IR';
    const decimals = parseInt(root.dataset.decimals || '2', 10);
    const grouping = root.dataset.grouping !== 'false';
    const min = root.dataset.min ? Number(root.dataset.min) : null;
    const max = root.dataset.max ? Number(root.dataset.max) : null;

    const step = root.dataset.step
        ? Number(root.dataset.step)
        : (decimals > 0 ? Math.pow(10, -decimals) : 1);

    const format = techCreateNumberFormat(culture, grouping, decimals);
    const separators = techNumberSeparators(format);

    function writeRaw(number) {

        if (!hidden) return;

        hidden.value = number === null ? '' : String(number);
    }

    function clamp(number) {

        let result = number;

        if (min !== null && result < min) result = min;
        if (max !== null && result > max) result = max;

        return result;
    }

    function display(number) {

        if (number === null) {
            visible.value = '';
            writeRaw(null);
            return;
        }

        const rounded = techRoundNumber(clamp(number), decimals);

        visible.value = format.format(rounded);
        writeRaw(rounded);
    }

    function parseCurrent() {

        return techParseNumber(visible.value, separators);
    }

    function initialValue() {

        const source = root.dataset.value || (hidden ? hidden.value : '');

        if (!source) return null;

        return techParseNumber(source, separators);
    }

    display(initialValue());

    visible.addEventListener('focus', function () {

        const number = parseCurrent();

        if (number === null) return;

        visible.value = String(number)
            .replace('.', separators.decimal);

        visible.select();
    });

    visible.addEventListener('input', function () {

        const number = parseCurrent();

        writeRaw(number === null ? null : techRoundNumber(
            clamp(number), decimals));
    });

    visible.addEventListener('blur', function () {

        display(parseCurrent());
    });

    visible.addEventListener('change', function () {

        Tech.trigger(root, 'Change', {
            sender: visible,
            value: hidden ? hidden.value : visible.value
        });
    });

    root.querySelectorAll('[data-spin]')
        .forEach(function (button) {

            button.addEventListener('mousedown', function (ev) {
                ev.preventDefault();
            });

            button.addEventListener('click', function () {

                const current = parseCurrent();

                const next = current === null
                    ? (min !== null ? min : 0)
                    : current + (button.dataset.spin === 'up' ? step : -step);

                visible.focus();

                display(next);

                visible.select();

                Tech.trigger(root, 'Change', {
                    sender: visible,
                    value: hidden ? hidden.value : visible.value
                });
            });
        });
}

function techParseMask(mask) {

    const tokens = [];

    for (let i = 0; i < mask.length; i++) {

        const ch = mask[i];

        if (ch === '\\' && i + 1 < mask.length) {
            tokens.push({ type: 'literal', char: mask[++i] });
            continue;
        }

        if ('09aA*'.indexOf(ch) >= 0) {
            tokens.push({ type: ch });
            continue;
        }

        tokens.push({ type: 'literal', char: ch });
    }

    return tokens;
}

function techMaskTokenMatches(type, ch) {

    switch (type) {
        case '0': return /[0-9]/.test(ch);
        case '9': return /[0-9 ]/.test(ch);
        case 'a': return /[A-Za-z]/.test(ch);
        case 'A': return /[A-Za-z0-9]/.test(ch);
        case '*': return ch.trim().length > 0;
        default: return false;
    }
}

function techMaskExtract(text, tokens) {

    let raw = '';
    let index = 0;

    for (const token of tokens) {

        if (index >= text.length) break;

        if (token.type === 'literal') {

            if (text[index] === token.char) index++;

            continue;
        }

        while (index < text.length &&
            !techMaskTokenMatches(token.type, text[index])) {
            index++;
        }

        if (index >= text.length) break;

        raw += text[index++];
    }

    return raw;
}

function techMaskRender(raw, tokens, caretRaw) {

    let text = '';
    let index = 0;
    let caret = null;

    for (const token of tokens) {

        if (token.type === 'literal') {

            if (index < raw.length) {

                if (caret === null && caretRaw >= 0 && index === caretRaw) {
                    caret = text.length;
                }

                text += token.char;
            }

            continue;
        }

        if (index >= raw.length) break;

        if (caret === null && caretRaw >= 0 && index === caretRaw) {
            caret = text.length;
        }

        text += raw[index++];
    }

    return {
        text: text,
        caret: caret === null ? text.length : caret
    };
}

function initMasked(root) {

    const visible = root.querySelector('input.tech-input-control');
    const hidden = root.querySelector('input[type="hidden"]');

    if (!visible || visible._techMasked) return;

    visible._techMasked = true;

    const tokens = techParseMask(root.dataset.mask || '');

    if (tokens.length === 0) return;

    function apply(keepCaret) {

        const caret = keepCaret ? visible.selectionStart : -1;

        const text = techNormalizeDigits(visible.value);

        const raw = techMaskExtract(text, tokens);

        const caretRaw = keepCaret && caret !== null
            ? techMaskExtract(text.slice(0, caret), tokens).length
            : -1;

        const rendered = techMaskRender(raw, tokens, caretRaw);

        visible.value = rendered.text;

        if (hidden) hidden.value = raw;

        if (keepCaret && caret !== null) {

            const position = Math.min(rendered.caret, visible.value.length);

            visible.setSelectionRange(position, position);
        }
    }

    apply(false);

    visible.addEventListener('input', function () {
        apply(true);
    });

    visible.addEventListener('change', function () {

        Tech.trigger(root, 'Change', {
            sender: visible,
            value: hidden ? hidden.value : visible.value
        });
    });
}

Tech.register('textbox', initTextbox);
Tech.register('textarea', initTextarea);
Tech.register('numerictextbox', initNumeric);
Tech.register('maskedtextbox', initMasked);

// ============================================================
// DatePicker (Jalali calendar)
// ============================================================

const techJalaliMonthNames = [
    'فروردین', 'اردیبهشت', 'خرداد', 'تیر', 'مرداد', 'شهریور',
    'مهر', 'آبان', 'آذر', 'دی', 'بهمن', 'اسفند'
];

const techJalaliWeekDays = [
    'شنبه', 'یکشنبه', 'دوشنبه', 'سه‌شنبه', 'چهارشنبه', 'پنجشنبه', 'جمعه'
];

const techGregorianMonthNames = [
    'ژانویه', 'فوریه', 'مارس', 'آوریل', 'مه', 'ژوئن',
    'ژوئیه', 'اوت', 'سپتامبر', 'اکتبر', 'نوامبر', 'دسامبر'
];

function techDiv(a, b) {
    return Math.trunc(a / b);
}

function techMod(a, b) {
    return a - Math.trunc(a / b) * b;
}

function techPad(value) {
    return value < 10 ? '0' + value : String(value);
}

function techJalCal(jy) {

    const breaks = [
        -61, 9, 38, 199, 426, 686, 756, 818, 1111, 1181, 1210,
        1635, 2060, 2097, 2192, 2262, 2324, 2394, 2456, 3178
    ];

    const bl = breaks.length;
    const gy = jy + 621;

    let leapJ = -14;
    let jp = breaks[0];
    let jump = 0;

    for (let i = 1; i < bl; i++) {

        const jm = breaks[i];

        jump = jm - jp;

        if (jy < jm) break;

        leapJ = leapJ + techDiv(jump, 33) * 8 + techDiv(techMod(jump, 33), 4);
        jp = jm;
    }

    let n = jy - jp;

    leapJ = leapJ + techDiv(n, 33) * 8 + techDiv(techMod(n, 33) + 3, 4);

    if (techMod(jump, 33) === 4 && jump - n === 4) leapJ++;

    const leapG = techDiv(gy, 4) - techDiv((techDiv(gy, 100) + 1) * 3, 4) - 150;
    const march = 20 + leapJ - leapG;

    if (jump - n < 6) n = n - jump + techDiv(jump + 4, 33) * 33;

    let leap = techMod(techMod(n + 1, 33) - 1, 4);

    if (leap === -1) leap = 4;

    return { leap: leap, gy: gy, march: march };
}

function techG2d(gy, gm, gd) {

    let d = techDiv((gy + techDiv(gm - 8, 6) + 100100) * 1461, 4)
        + techDiv(153 * techMod(gm + 9, 12) + 2, 5)
        + gd - 34840408;

    d = d - techDiv(techDiv(gy + 100100 + techDiv(gm - 8, 6), 100) * 3, 4) + 752;

    return d;
}

function techD2g(jdn) {

    let j = 4 * jdn + 139361631;

    j = j + techDiv(techDiv(4 * jdn + 183187720, 146097) * 3, 4) * 4 - 3908;

    const i = techDiv(techMod(j, 1461), 4) * 5 + 308;
    const gd = techDiv(techMod(i, 153), 5) + 1;
    const gm = techMod(techDiv(i, 153), 12) + 1;
    const gy = techDiv(j, 1461) - 100100 + techDiv(8 - gm, 6);

    return { gy: gy, gm: gm, gd: gd };
}

function techJ2d(jy, jm, jd) {

    const cal = techJalCal(jy);

    return techG2d(cal.gy, 3, cal.march)
        + (jm - 1) * 31
        - techDiv(jm, 7) * (jm - 7)
        + jd - 1;
}

function techD2j(jdn) {

    const gy = techD2g(jdn).gy;

    let jy = gy - 621;

    const cal = techJalCal(jy);
    const jdn1f = techG2d(gy, 3, cal.march);

    let k = jdn - jdn1f;

    if (k >= 0) {

        if (k <= 185) {
            return {
                jy: jy,
                jm: 1 + techDiv(k, 31),
                jd: techMod(k, 31) + 1
            };
        }

        k -= 186;
    }
    else {
        jy -= 1;
        k += 179;

        if (cal.leap === 1) k += 1;
    }

    return {
        jy: jy,
        jm: 7 + techDiv(k, 30),
        jd: techMod(k, 30) + 1
    };
}

function techJalaliMonthLength(jy, jm) {

    if (jm <= 6) return 31;
    if (jm <= 11) return 30;

    return techJalCal(jy).leap === 0 ? 30 : 29;
}

function techJalaliToGregorian(jy, jm, jd) {

    return techD2g(techJ2d(jy, jm, jd));
}

function techGregorianToJalali(gy, gm, gd) {

    return techD2j(techG2d(gy, gm, gd));
}

function techIsoParts(iso) {

    const match = /^(\d{4})-(\d{2})-(\d{2})$/.exec(
        String(iso || '').trim());

    if (!match) return null;

    return { y: +match[1], m: +match[2], d: +match[3] };
}

function techIsoDate(gy, gm, gd) {

    return gy + '-' + techPad(gm) + '-' + techPad(gd);
}

function techFormatJalali(jy, jm, jd, format) {

    return (format || 'yyyy/MM/dd')
        .replace(/yyyy/g, String(jy))
        .replace(/MM/g, techPad(jm))
        .replace(/dd/g, techPad(jd));
}

function techFormatGregorian(gy, gm, gd, format) {

    return (format || 'yyyy/MM/dd')
        .replace(/yyyy/g, String(gy))
        .replace(/MM/g, techPad(gm))
        .replace(/dd/g, techPad(gd));
}

function techGregorianDate(gy, gm, gd) {

    const date = new Date(0);

    date.setHours(0, 0, 0, 0);
    date.setFullYear(gy, gm - 1, gd);

    return date;
}

function techDatePickerMode(root) {

    return root._techDateState && root._techDateState.mode === 'gregorian'
        ? 'gregorian'
        : 'jalali';
}

function techCalendarOption(root) {

    const calendar = String(root.dataset.calendar || '').toLowerCase();

    return calendar === 'jalali' || calendar === 'gregorian'
        ? calendar
        : 'both';
}

function techDatePickerDisplay(root, parts) {

    if (techDatePickerMode(root) === 'gregorian') {

        return techFormatGregorian(
            parts.y, parts.m, parts.d, root.dataset.format);
    }

    const jalali = techGregorianToJalali(parts.y, parts.m, parts.d);

    return techFormatJalali(
        jalali.jy, jalali.jm, jalali.jd, root.dataset.format);
}

Tech.DatePicker = Tech.DatePicker || {};
Tech.DatePicker.instances = Tech.DatePicker.instances || new Map();

Tech.DatePicker.get = function (id) {
    return Tech.DatePicker.instances.get(id) || null;
};

Tech.DatePicker.value = function (id) {

    const instance = Tech.DatePicker.get(id);

    return instance ? instance.value() : null;
};

Tech.DatePicker.setValue = function (id, iso) {

    const instance = Tech.DatePicker.get(id);

    if (instance) instance.setValue(iso);
};

Tech.DatePicker.calendar = function (id) {

    const instance = Tech.DatePicker.get(id);

    return instance ? instance.calendar() : null;
};

Tech.DatePicker.setCalendar = function (id, mode) {

    const instance = Tech.DatePicker.get(id);

    if (instance) instance.setCalendar(mode);
};

function techRenderDatePicker(root, state) {

    const panel = root.querySelector('.tech-datepicker-panel');
    const hidden = root.querySelector('input[type="hidden"]');

    if (!panel || !hidden) return;

    const iso = hidden.value || '';
    const today = new Date();
    const todayIso = techIsoDate(
        today.getFullYear(), today.getMonth() + 1, today.getDate());

    const mode = state.mode === 'gregorian' ? 'gregorian' : 'jalali';
    const min = root.dataset.min || '';
    const max = root.dataset.max || '';

    let title;
    let length;
    let offset;

    if (mode === 'gregorian') {

        length = techGregorianDate(state.gy, state.gm + 1, 0).getDate();
        offset = (techGregorianDate(state.gy, state.gm, 1).getDay() + 1) % 7;
        title = techGregorianMonthNames[state.gm - 1] + ' ' + state.gy;
    }
    else {

        length = techJalaliMonthLength(state.jy, state.jm);

        const first = techJalaliToGregorian(state.jy, state.jm, 1);

        offset = (new Date(first.gy, first.gm - 1, first.gd)
            .getDay() - 6 + 7) % 7;
        title = techJalaliMonthNames[state.jm - 1] + ' ' + state.jy;
    }

    let html = '<div class="tech-dp-header">';

    if (techCalendarOption(root) === 'both') {

        const label = mode === 'gregorian' ? 'میلادی' : 'شمسی';
        const next = mode === 'gregorian' ? 'شمسی' : 'میلادی';

        html += '<button type="button" class="tech-dp-mode" data-action="calendar"'
            + ' title="تغییر به تقویم ' + next + '"'
            + ' aria-label="تقویم ' + label + '؛ تغییر به تقویم ' + next + '">'
            + label + '</button>';
    }

    html += '<button type="button" class="tech-dp-nav" data-nav="py" aria-label="سال قبل">«</button>'
        + '<button type="button" class="tech-dp-nav" data-nav="pm" aria-label="ماه قبل">‹</button>'
        + '<span class="tech-dp-title">' + title + '</span>'
        + '<button type="button" class="tech-dp-nav" data-nav="nm" aria-label="ماه بعد">›</button>'
        + '<button type="button" class="tech-dp-nav" data-nav="ny" aria-label="سال بعد">»</button>'
        + '</div>';

    html += '<table class="tech-dp-grid"><thead><tr>';

    techJalaliWeekDays.forEach(function (name) {
        html += '<th>' + name + '</th>';
    });

    html += '</tr></thead><tbody><tr>';

    let cell = 0;

    for (let i = 0; i < offset; i++) {
        html += '<td></td>';
        cell++;
    }

    for (let day = 1; day <= length; day++) {

        if (cell > 0 && cell % 7 === 0) html += '</tr><tr>';

        let dayIso;

        if (mode === 'gregorian') {
            dayIso = techIsoDate(state.gy, state.gm, day);
        }
        else {
            const gregorian = techJalaliToGregorian(state.jy, state.jm, day);

            dayIso = techIsoDate(
                gregorian.gy, gregorian.gm, gregorian.gd);
        }

        const classes = ['tech-dp-day'];

        if ((offset + day - 1) % 7 === 6) classes.push('is-weekend');

        if (dayIso === iso) classes.push('is-selected');

        if (dayIso === todayIso) classes.push('is-today');

        if ((min && dayIso < min) || (max && dayIso > max)) {
            classes.push('is-disabled');
        }

        html += '<td><button type="button" class="' + classes.join(' ')
            + '" data-day="' + dayIso + '">' + day + '</button></td>';

        cell++;
    }

    while (cell % 7 !== 0) {
        html += '<td></td>';
        cell++;
    }

    html += '</tr></tbody></table>';

    if (root.dataset.today !== 'false' || root.dataset.clear !== 'false') {

        html += '<div class="tech-dp-footer">';

        if (root.dataset.today !== 'false') {
            html += '<button type="button" class="tech-dp-action" data-action="today">امروز</button>';
        }

        if (root.dataset.clear !== 'false') {
            html += '<button type="button" class="tech-dp-action" data-action="clear">پاک کردن</button>';
        }

        html += '</div>';
    }

    panel.innerHTML = html;
}

function techSetDatePickerValue(root, iso, fire) {

    const hidden = root.querySelector('input[type="hidden"]');
    const input = root.querySelector('input.tech-input-control');

    if (!hidden || !input) return;

    const parts = techIsoParts(iso);

    hidden.value = parts ? iso : '';
    input.value = parts ? techDatePickerDisplay(root, parts) : '';

    if (fire) {
        Tech.trigger(root, 'Change', {
            sender: input,
            value: hidden.value,
            display: input.value,
            calendar: techDatePickerMode(root)
        });
    }
}

function techSetDatePickerCalendar(root, mode) {

    const state = root._techDateState;

    if (!state) return;

    const calendar = techCalendarOption(root);

    const next = calendar === 'both'
        ? (mode === 'gregorian' ? 'gregorian' : 'jalali')
        : calendar;

    if (state.mode === next) return;

    const hidden = root.querySelector('input[type="hidden"]');
    const input = root.querySelector('input.tech-input-control');

    const selected = techIsoParts(hidden ? hidden.value : '');
    const today = new Date();

    const anchor = selected || {
        y: today.getFullYear(),
        m: today.getMonth() + 1,
        d: today.getDate()
    };

    state.mode = next;

    if (next === 'gregorian') {
        state.gy = anchor.y;
        state.gm = anchor.m;
    }
    else {
        const jalali = techGregorianToJalali(anchor.y, anchor.m, anchor.d);

        state.jy = jalali.jy;
        state.jm = jalali.jm;
    }

    if (hidden && input) {
        const parts = techIsoParts(hidden.value);

        input.value = parts ? techDatePickerDisplay(root, parts) : '';
    }

    if (root.classList.contains('open')) {
        techRenderDatePicker(root, state);
    }
}

function techOpenDatePicker(root) {

    const hidden = root.querySelector('input[type="hidden"]');
    const input = root.querySelector('input.tech-input-control');
    const state = root._techDateState;

    if (!hidden || !input || !state) return;

    document.querySelectorAll('.tech-datepicker.open')
        .forEach(function (other) {
            if (other !== root) techCloseDatePicker(other);
        });

    const selected = techIsoParts(hidden.value);
    const today = new Date();

    const anchor = selected || {
        y: today.getFullYear(),
        m: today.getMonth() + 1,
        d: today.getDate()
    };

    if (state.mode === 'gregorian') {
        state.gy = anchor.y;
        state.gm = anchor.m;
    }
    else {
        const jalali = techGregorianToJalali(
            anchor.y, anchor.m, anchor.d);

        state.jy = jalali.jy;
        state.jm = jalali.jm;
    }

    techRenderDatePicker(root, state);

    root.classList.add('open');

    const panel = root.querySelector('.tech-datepicker-panel');

    if (panel) {
        techFloatPopup(root, panel, { anchor: root });
    }

    input.setAttribute('aria-expanded', 'true');
}

function techCloseDatePicker(root) {

    techReleasePopup(root);

    root.classList.remove('open');

    const input = root.querySelector('input.tech-input-control');

    if (input) input.setAttribute('aria-expanded', 'false');
}

function initDatePicker(root) {

    const input = root.querySelector('input.tech-input-control');
    const hidden = root.querySelector('input[type="hidden"]');
    const panel = root.querySelector('.tech-datepicker-panel');

    if (!input || !hidden || !panel || input._techDatePicker) return;

    input._techDatePicker = true;

    const today = new Date();
    const todayJ = techGregorianToJalali(
        today.getFullYear(), today.getMonth() + 1, today.getDate());

    root._techDateState = {
        mode: techCalendarOption(root) === 'gregorian'
            ? 'gregorian'
            : 'jalali',
        jy: todayJ.jy,
        jm: todayJ.jm,
        gy: today.getFullYear(),
        gm: today.getMonth() + 1
    };

    const initial = hidden.value || root.dataset.value || '';

    if (initial) techSetDatePickerValue(root, initial, false);

    function open() {

        if (input.disabled) return;

        techOpenDatePicker(root);
    }

    input.addEventListener('focus', function () {

        if (root.dataset.openOnFocus === 'false') return;
        if (root._techSuppressOpen) return;

        open();
    });

    input.addEventListener('click', open);

    const toggle = root.querySelector('.tech-datepicker-toggle');

    if (toggle) {
        toggle.addEventListener('click', function () {

            if (root.classList.contains('open')) {
                techCloseDatePicker(root);
            }
            else {
                open();
            }
        });
    }

    panel.addEventListener('click', function (ev) {

        ev.stopPropagation();

        const nav = ev.target.closest('[data-nav]');

        if (nav) {

            const state = root._techDateState;

            if (state.mode === 'gregorian') {

                switch (nav.dataset.nav) {
                    case 'pm':
                        state.gm--;
                        if (state.gm < 1) { state.gm = 12; state.gy--; }
                        break;
                    case 'nm':
                        state.gm++;
                        if (state.gm > 12) { state.gm = 1; state.gy++; }
                        break;
                    case 'py':
                        state.gy--;
                        break;
                    case 'ny':
                        state.gy++;
                        break;
                }

                if (state.gy < 1) { state.gy = 1; state.gm = 1; }
                if (state.gy > 9999) { state.gy = 9999; state.gm = 12; }
            }
            else {

                switch (nav.dataset.nav) {
                    case 'pm':
                        state.jm--;
                        if (state.jm < 1) { state.jm = 12; state.jy--; }
                        break;
                    case 'nm':
                        state.jm++;
                        if (state.jm > 12) { state.jm = 1; state.jy++; }
                        break;
                    case 'py':
                        state.jy--;
                        break;
                    case 'ny':
                        state.jy++;
                        break;
                }

                if (state.jy < 1200) { state.jy = 1200; state.jm = 1; }
                if (state.jy > 1600) { state.jy = 1600; state.jm = 12; }
            }

            techRenderDatePicker(root, state);

            return;
        }

        const day = ev.target.closest('[data-day]');

        if (day) {

            if (day.classList.contains('is-disabled')) return;

            techSetDatePickerValue(root, day.dataset.day, true);

            techCloseDatePicker(root);

            root._techSuppressOpen = true;
            input.focus();
            root._techSuppressOpen = false;

            return;
        }

        const action = ev.target.closest('[data-action]');

        if (!action) return;

        if (action.dataset.action === 'calendar') {

            techSetDatePickerCalendar(
                root,
                techDatePickerMode(root) === 'gregorian'
                    ? 'jalali'
                    : 'gregorian');

            return;
        }

        if (action.dataset.action === 'today') {

            techSetDatePickerValue(
                root,
                techIsoDate(
                    today.getFullYear(),
                    today.getMonth() + 1,
                    today.getDate()),
                true);
        }
        else {
            techSetDatePickerValue(root, '', true);
        }

        techCloseDatePicker(root);
    });

    Tech.DatePicker.instances.set(input.id, {
        element: root,
        value: function () { return hidden.value; },
        setValue: function (iso) {
            techSetDatePickerValue(root, iso || '', true);
        },
        calendar: function () { return techDatePickerMode(root); },
        setCalendar: function (mode) { techSetDatePickerCalendar(root, mode); },
        open: open,
        close: function () { techCloseDatePicker(root); }
    });
}

Tech.register('datepicker', initDatePicker);

// ============================================================
// DateRangePicker (Jalali)
// ============================================================

Tech.DateRangePicker = Tech.DateRangePicker || {};
Tech.DateRangePicker.instances = Tech.DateRangePicker.instances || new Map();

Tech.DateRangePicker.get = function (id) {

    return Tech.DateRangePicker.instances.get(id) || null;
};

Tech.DateRangePicker.value = function (id) {

    const instance = Tech.DateRangePicker.get(id);

    return instance ? instance.value() : null;
};

Tech.DateRangePicker.setValue = function (id, startIso, endIso) {

    const instance = Tech.DateRangePicker.get(id);

    if (instance) instance.setValue(startIso, endIso);
};

Tech.DateRangePicker.calendar = function (id) {

    const instance = Tech.DateRangePicker.get(id);

    return instance ? instance.calendar() : null;
};

Tech.DateRangePicker.setCalendar = function (id, mode) {

    const instance = Tech.DateRangePicker.get(id);

    if (instance) instance.setCalendar(mode);
};

function techDateRangeMode(root) {

    return root._techRangeState && root._techRangeState.mode === 'gregorian'
        ? 'gregorian'
        : 'jalali';
}

function techRangeDayIso(mode, year, month, day) {

    if (mode === 'gregorian') {

        return techIsoDate(year, month, day);
    }

    const gregorian = techJalaliToGregorian(year, month, day);

    return techIsoDate(gregorian.gy, gregorian.gm, gregorian.gd);
}

function techRangeMonthLength(mode, year, month) {

    return mode === 'gregorian'
        ? techGregorianDate(year, month + 1, 0).getDate()
        : techJalaliMonthLength(year, month);
}

function techRangeMonthOffset(mode, year, month) {

    if (mode === 'gregorian') {

        return (techGregorianDate(year, month, 1).getDay() + 1) % 7;
    }

    const first = techJalaliToGregorian(year, month, 1);

    return (new Date(first.gy, first.gm - 1, first.gd)
        .getDay() - 6 + 7) % 7;
}

function techRangeDisplay(root, iso) {

    const parts = techIsoParts(iso);

    if (!parts) return '';

    if (techDateRangeMode(root) === 'gregorian') {

        return techFormatGregorian(
            parts.y, parts.m, parts.d, root.dataset.format);
    }

    const jalali = techGregorianToJalali(parts.y, parts.m, parts.d);

    return techFormatJalali(
        jalali.jy, jalali.jm, jalali.jd, root.dataset.format);
}

function techRenderDateRange(root) {

    const panel = root.querySelector('.tech-daterange-panel');
    const state = root._techRangeState;

    if (!panel || !state) return;

    const today = new Date();
    const todayIso = techIsoDate(
        today.getFullYear(), today.getMonth() + 1, today.getDate());

    const min = root.dataset.min || '';
    const max = root.dataset.max || '';

    const mode = techDateRangeMode(root);

    const start = state.start;
    const end = state.end;
    const preview = start && !end ? state.hover : null;
    const rangeEnd = end || preview;

    const months = [];

    for (let index = 0; index < 2; index++) {

        let year = mode === 'gregorian' ? state.gy : state.jy;
        let month = (mode === 'gregorian' ? state.gm : state.jm) + index;

        while (month > 12) {
            month -= 12;
            year++;
        }

        months.push({ year: year, month: month });
    }

    const monthNames = mode === 'gregorian'
        ? techGregorianMonthNames
        : techJalaliMonthNames;

    let html = '<div class="tech-dr-header">';

    if (techCalendarOption(root) === 'both') {

        const label = mode === 'gregorian' ? 'میلادی' : 'شمسی';
        const next = mode === 'gregorian' ? 'شمسی' : 'میلادی';

        html += '<button type="button" class="tech-dp-mode" data-action="calendar"'
            + ' title="تغییر به تقویم ' + next + '"'
            + ' aria-label="تقویم ' + label + '؛ تغییر به تقویم ' + next + '">'
            + label + '</button>';
    }

    html += '<button type="button" class="tech-dp-nav" data-nav="py" aria-label="سال قبل">«</button>'
        + '<button type="button" class="tech-dp-nav" data-nav="pm" aria-label="ماه قبل">‹</button>'
        + '<span class="tech-dp-title">'
        + monthNames[months[0].month - 1] + ' ' + months[0].year
        + ' – '
        + monthNames[months[1].month - 1] + ' ' + months[1].year
        + '</span>'
        + '<button type="button" class="tech-dp-nav" data-nav="nm" aria-label="ماه بعد">›</button>'
        + '<button type="button" class="tech-dp-nav" data-nav="ny" aria-label="سال بعد">»</button>'
        + '</div>';

    html += '<div class="tech-dr-months">';

    months.forEach(function (month) {

        const length = techRangeMonthLength(
            mode, month.year, month.month);

        const offset = techRangeMonthOffset(
            mode, month.year, month.month);

        html += '<div class="tech-dr-month">'
            + '<table class="tech-dp-grid"><thead><tr>';

        techJalaliWeekDays.forEach(function (name) {
            html += '<th>' + name + '</th>';
        });

        html += '</tr></thead><tbody><tr>';

        let cell = 0;

        for (; cell < offset; cell++) {
            html += '<td></td>';
        }

        for (let day = 1; day <= length; day++) {

            if (cell > 0 && cell % 7 === 0) html += '</tr><tr>';

            const dayIso = techRangeDayIso(
                mode, month.year, month.month, day);

            const classes = ['tech-dp-day'];

            if ((offset + day - 1) % 7 === 6) classes.push('is-weekend');

            if (dayIso === start || dayIso === end) {
                classes.push('is-selected');
            }
            else if (start && rangeEnd &&
                dayIso > start && dayIso < rangeEnd) {
                classes.push('is-in-range');
            }

            if (dayIso === todayIso) {
                classes.push('is-today');
            }

            if ((min && dayIso < min) || (max && dayIso > max)) {
                classes.push('is-disabled');
            }

            html += '<td><button type="button" class="' + classes.join(' ')
                + '" data-day="' + dayIso + '">' + day + '</button></td>';

            cell++;
        }

        while (cell % 7 !== 0) {
            html += '<td></td>';
            cell++;
        }

        html += '</tr></tbody></table></div>';
    });

    html += '</div>';

    let summary = 'تاریخ شروع را انتخاب کنید.';

    if (start && end) {
        summary = 'از ' + techRangeDisplay(root, start)
            + ' تا ' + techRangeDisplay(root, end);
    }
    else if (start) {
        summary = 'از ' + techRangeDisplay(root, start)
            + ' — تاریخ پایان را انتخاب کنید.';
    }

    html += '<div class="tech-dr-footer">'
        + '<span class="tech-dr-summary">' + summary + '</span>'
        + '<span class="tech-dr-actions">';

    if (root.dataset.today !== 'false') {
        html += '<button type="button" class="tech-dp-action" data-action="today">امروز</button>';
    }

    if (root.dataset.clear !== 'false') {
        html += '<button type="button" class="tech-dp-action" data-action="clear">پاک کردن</button>';
    }

    html += '</span></div>';

    panel.innerHTML = html;
}

function techSetDateRangeValue(root, startIso, endIso, fire) {

    const hidden = root.querySelectorAll('input[type="hidden"]');
    const startInput = root.querySelector('.tech-daterange-start');
    const endInput = root.querySelector('.tech-daterange-end');
    const state = root._techRangeState;

    if (hidden.length < 2 || !startInput || !endInput || !state) return;

    let start = techIsoParts(startIso) ? startIso : '';
    let end = techIsoParts(endIso) ? endIso : '';

    if (!start && end) {
        start = end;
        end = '';
    }

    if (start && end && start > end) {
        const swap = start;
        start = end;
        end = swap;
    }

    hidden[0].value = start;
    hidden[1].value = end;

    startInput.value = techRangeDisplay(root, start);
    endInput.value = techRangeDisplay(root, end);

    root.dataset.start = start;
    root.dataset.end = end;

    state.start = start || null;
    state.end = end || null;
    state.hover = null;

    if (fire) {
        Tech.trigger(root, 'Change', {
            sender: startInput,
            start: start,
            end: end,
            calendar: techDateRangeMode(root)
        });
    }
}

function techSetDateRangeCalendar(root, mode) {

    const state = root._techRangeState;

    if (!state) return;

    const calendar = techCalendarOption(root);

    const next = calendar === 'both'
        ? (mode === 'gregorian' ? 'gregorian' : 'jalali')
        : calendar;

    if (state.mode === next) return;

    const today = new Date();
    const selected = techIsoParts(state.start);

    const anchor = selected || {
        y: today.getFullYear(),
        m: today.getMonth() + 1,
        d: today.getDate()
    };

    state.mode = next;

    if (next === 'gregorian') {
        state.gy = anchor.y;
        state.gm = anchor.m;
    }
    else {
        const jalali = techGregorianToJalali(anchor.y, anchor.m, anchor.d);

        state.jy = jalali.jy;
        state.jm = jalali.jm;
    }

    const startInput = root.querySelector('.tech-daterange-start');
    const endInput = root.querySelector('.tech-daterange-end');

    if (startInput) startInput.value = techRangeDisplay(root, state.start);
    if (endInput) endInput.value = techRangeDisplay(root, state.end);

    if (root.classList.contains('open')) {
        techRenderDateRange(root);
    }
}

function techOpenDateRange(root) {

    const panel = root.querySelector('.tech-daterange-panel');
    const state = root._techRangeState;

    if (!panel || !state) return;

    document.querySelectorAll('.tech-daterange.open')
        .forEach(function (other) {
            if (other !== root) techCloseDateRange(other);
        });

    document.querySelectorAll('.tech-datepicker.open')
        .forEach(techCloseDatePicker);

    const selected = techIsoParts(state.start);

    if (selected) {

        if (state.mode === 'gregorian') {
            state.gy = selected.y;
            state.gm = selected.m;
        }
        else {
            const jalali = techGregorianToJalali(
                selected.y, selected.m, selected.d);

            state.jy = jalali.jy;
            state.jm = jalali.jm;
        }
    }

    techRenderDateRange(root);

    root.classList.add('open');

    techFloatPopup(root, panel, { anchor: root });

    root.querySelectorAll(
        '.tech-daterange-start, .tech-daterange-end')
        .forEach(function (input) {
            input.setAttribute('aria-expanded', 'true');
        });
}

function techCloseDateRange(root) {

    techReleasePopup(root);

    root.classList.remove('open');

    root.querySelectorAll(
        '.tech-daterange-start, .tech-daterange-end')
        .forEach(function (input) {
            input.setAttribute('aria-expanded', 'false');
        });
}

function initDateRangePicker(root) {

    const hidden = root.querySelectorAll('input[type="hidden"]');
    const startInput = root.querySelector('.tech-daterange-start');
    const endInput = root.querySelector('.tech-daterange-end');
    const panel = root.querySelector('.tech-daterange-panel');

    if (hidden.length < 2 || !startInput || !endInput || !panel ||
        root._techRangeReady) {
        return;
    }

    root._techRangeReady = true;

    const today = new Date();
    const todayJ = techGregorianToJalali(
        today.getFullYear(), today.getMonth() + 1, today.getDate());

    root._techRangeState = {
        mode: techCalendarOption(root) === 'gregorian'
            ? 'gregorian'
            : 'jalali',
        jy: todayJ.jy,
        jm: todayJ.jm,
        gy: today.getFullYear(),
        gm: today.getMonth() + 1,
        start: null,
        end: null,
        hover: null
    };

    techSetDateRangeValue(
        root,
        hidden[0].value || root.dataset.start || '',
        hidden[1].value || root.dataset.end || '',
        false);

    function open() {

        if (startInput.disabled) return;

        techOpenDateRange(root);
    }

    [startInput, endInput].forEach(function (input) {

        input.addEventListener('focus', function () {

            if (root.dataset.openOnFocus === 'false') return;
            if (root._techSuppressOpen) return;

            open();
        });

        input.addEventListener('click', open);
    });

    const toggle = root.querySelector('.tech-daterange-toggle');

    if (toggle) {
        toggle.addEventListener('click', function () {

            if (root.classList.contains('open')) {
                techCloseDateRange(root);
            }
            else {
                open();
            }
        });
    }

    panel.addEventListener('click', function (ev) {

        ev.stopPropagation();

        const nav = ev.target.closest('[data-nav]');

        if (nav) {

            const state = root._techRangeState;

            if (state.mode === 'gregorian') {

                switch (nav.dataset.nav) {
                    case 'pm':
                        state.gm--;
                        if (state.gm < 1) { state.gm = 12; state.gy--; }
                        break;
                    case 'nm':
                        state.gm++;
                        if (state.gm > 12) { state.gm = 1; state.gy++; }
                        break;
                    case 'py':
                        state.gy--;
                        break;
                    case 'ny':
                        state.gy++;
                        break;
                }

                if (state.gy < 1) { state.gy = 1; state.gm = 1; }
                if (state.gy > 9999) { state.gy = 9999; state.gm = 12; }
            }
            else {

                switch (nav.dataset.nav) {
                    case 'pm':
                        state.jm--;
                        if (state.jm < 1) { state.jm = 12; state.jy--; }
                        break;
                    case 'nm':
                        state.jm++;
                        if (state.jm > 12) { state.jm = 1; state.jy++; }
                        break;
                    case 'py':
                        state.jy--;
                        break;
                    case 'ny':
                        state.jy++;
                        break;
                }

                if (state.jy < 1200) { state.jy = 1200; state.jm = 1; }
                if (state.jy > 1600) { state.jy = 1600; state.jm = 12; }
            }

            techRenderDateRange(root);

            return;
        }

        const day = ev.target.closest('[data-day]');

        if (day) {

            if (day.classList.contains('is-disabled')) return;

            const state = root._techRangeState;
            const iso = day.dataset.day;

            if (!state.start || state.end) {

                techSetDateRangeValue(root, iso, '', false);
                techRenderDateRange(root);

                return;
            }

            let start = state.start;
            let end = iso;

            if (iso < start) {
                end = start;
                start = iso;
            }

            techSetDateRangeValue(root, start, end, true);

            techCloseDateRange(root);

            root._techSuppressOpen = true;
            startInput.focus();
            root._techSuppressOpen = false;

            return;
        }

        const action = ev.target.closest('[data-action]');

        if (!action) return;

        if (action.dataset.action === 'calendar') {

            techSetDateRangeCalendar(
                root,
                techDateRangeMode(root) === 'gregorian'
                    ? 'jalali'
                    : 'gregorian');

            return;
        }

        if (action.dataset.action === 'today') {

            const iso = techIsoDate(
                today.getFullYear(),
                today.getMonth() + 1,
                today.getDate());

            techSetDateRangeValue(root, iso, iso, true);
        }
        else {
            techSetDateRangeValue(root, '', '', true);
        }

        techCloseDateRange(root);
    });

    panel.addEventListener('mouseover', function (ev) {

        const day = ev.target.closest('[data-day]');
        const state = root._techRangeState;

        if (!day || !state || !state.start || state.end) return;
        if (day.classList.contains('is-disabled')) return;

        const iso = day.dataset.day;

        if (state.hover === iso) return;

        state.hover = iso;

        techRenderDateRange(root);
    });

    panel.addEventListener('mouseleave', function () {

        const state = root._techRangeState;

        if (!state || !state.hover) return;

        state.hover = null;

        techRenderDateRange(root);
    });

    const instanceId = startInput.id || root.id;

    Tech.DateRangePicker.instances.set(instanceId, {
        element: root,
        value: function () {
            return { start: hidden[0].value, end: hidden[1].value };
        },
        setValue: function (startIso, endIso) {

            techSetDateRangeValue(
                root, startIso || '', endIso || '', true);

            techRenderDateRange(root);
        },
        calendar: function () { return techDateRangeMode(root); },
        setCalendar: function (mode) { techSetDateRangeCalendar(root, mode); },
        open: open,
        close: function () { techCloseDateRange(root); }
    });
}

Tech.register('daterange', initDateRangePicker);

// ============================================================
// TimePicker
// ============================================================

Tech.TimePicker = Tech.TimePicker || {};
Tech.TimePicker.instances = Tech.TimePicker.instances || new Map();

Tech.TimePicker.get = function (id) {
    return Tech.TimePicker.instances.get(id) || null;
};

Tech.TimePicker.value = function (id) {

    const instance = Tech.TimePicker.get(id);

    return instance ? instance.value() : null;
};

Tech.TimePicker.setValue = function (id, text) {

    const instance = Tech.TimePicker.get(id);

    if (instance) instance.setValue(text);
};

function techParseTimeValue(text) {

    const match = /^(\d{1,2}):(\d{2})(?::(\d{2}))?$/.exec(
        techNormalizeDigits(text || '').trim());

    if (!match) return null;

    const hours = +match[1];
    const minutes = +match[2];
    const seconds = match[3] ? +match[3] : 0;

    if (hours > 23 || minutes > 59 || seconds > 59) return null;

    return { h: hours, m: minutes, s: seconds };
}

function techTimeText(parts, withSeconds) {

    return techPad(parts.h) + ':' + techPad(parts.m)
        + (withSeconds ? ':' + techPad(parts.s) : '');
}

function techTimeTotal(parts) {
    return parts.h * 3600 + parts.m * 60 + parts.s;
}

function techFormatTimeDisplay(parts, use12, withSeconds) {

    if (!use12) return techTimeText(parts, withSeconds);

    const hour = parts.h % 12 === 0 ? 12 : parts.h % 12;

    return techPad(hour) + ':' + techPad(parts.m)
        + (withSeconds ? ':' + techPad(parts.s) : '')
        + ' ' + (parts.h < 12 ? 'ق.ظ' : 'ب.ظ');
}

function techClampTime(draft, min, max) {

    if (min && techTimeTotal(draft) < techTimeTotal(min)) {
        return { h: min.h, m: min.m, s: min.s };
    }

    if (max && techTimeTotal(draft) > techTimeTotal(max)) {
        return { h: max.h, m: max.m, s: max.s };
    }

    return draft;
}

function techTpOption(unit, value, label, selected, disabled) {

    return '<button type="button" class="tech-tp-option'
        + (selected ? ' is-selected' : '')
        + (disabled ? ' is-disabled' : '')
        + '" data-' + unit + '="' + value + '"'
        + (disabled ? ' disabled' : '')
        + '>' + label + '</button>';
}

function techRenderTimePicker(root) {

    const panel = root.querySelector('.tech-timepicker-panel');

    if (!panel) return;

    const state = root._techTimeState;

    const step = Math.max(1, parseInt(root.dataset.step || '1', 10));
    const withSeconds = root.dataset.seconds === 'true';
    const use12 = root.dataset.use12 === 'true';
    const min = techParseTimeValue(root.dataset.min);
    const max = techParseTimeValue(root.dataset.max);

    const draft = state.draft;
    const base = draft || { h: 0, m: 0, s: 0 };

    let html = '<div class="tech-tp-header"><span class="tech-tp-title">'
        + (draft ? techFormatTimeDisplay(draft, use12, withSeconds) : '—')
        + '</span></div>';

    if (use12) {

        html += '<div class="tech-tp-period">'
            + '<button type="button" class="tech-tp-period-btn'
            + (draft && draft.h < 12 ? ' is-selected' : '')
            + '" data-period="am">ق.ظ</button>'
            + '<button type="button" class="tech-tp-period-btn'
            + (draft && draft.h >= 12 ? ' is-selected' : '')
            + '" data-period="pm">ب.ظ</button>'
            + '</div>';
    }

    html += '<div class="tech-tp-body">';

    html += '<div class="tech-tp-column" data-unit="h">';

    for (let label = use12 ? 1 : 0; label <= (use12 ? 12 : 23); label++) {

        const hour = use12 ? (label % 12) + (base.h >= 12 ? 12 : 0) : label;

        const disabled = (min && hour < min.h) || (max && hour > max.h);

        html += techTpOption(
            'h', hour, label, !!draft && draft.h === hour, disabled);
    }

    html += '</div>';

    html += '<div class="tech-tp-column" data-unit="m">';

    for (let minute = 0; minute < 60; minute += step) {

        const disabled = (min && base.h === min.h && minute < min.m)
            || (max && base.h === max.h && minute > max.m);

        html += techTpOption(
            'm', minute, techPad(minute), !!draft && draft.m === minute, disabled);
    }

    html += '</div>';

    if (withSeconds) {

        html += '<div class="tech-tp-column" data-unit="s">';

        for (let second = 0; second < 60; second++) {

            const disabled = (min && base.h === min.h &&
                base.m === min.m && second < min.s)
                || (max && base.h === max.h &&
                    base.m === max.m && second > max.s);

            html += techTpOption(
                's', second, techPad(second),
                !!draft && draft.s === second, disabled);
        }

        html += '</div>';
    }

    html += '</div>';

    if (root.dataset.now !== 'false' || root.dataset.clear !== 'false') {

        html += '<div class="tech-tp-footer">';

        if (root.dataset.now !== 'false') {
            html += '<button type="button" class="tech-tp-action" data-action="now">اکنون</button>';
        }

        if (root.dataset.clear !== 'false') {
            html += '<button type="button" class="tech-tp-action" data-action="clear">پاک کردن</button>';
        }

        html += '</div>';
    }

    panel.innerHTML = html;

    panel.querySelectorAll('.tech-tp-column')
        .forEach(function (column) {

            const selected = column.querySelector('.is-selected');

            if (!selected) return;

            column.scrollTop = selected.offsetTop
                - column.clientHeight / 2
                + selected.clientHeight / 2;
        });
}

function techSetTimePickerValue(root, parts, fire) {

    const hidden = root.querySelector('input[type="hidden"]');
    const visible = root.querySelector('input.tech-input-control');

    if (!hidden || !visible) return;

    const withSeconds = root.dataset.seconds === 'true';
    const use12 = root.dataset.use12 === 'true';

    hidden.value = parts ? techTimeText(parts, withSeconds) : '';

    visible.value = parts
        ? techFormatTimeDisplay(parts, use12, withSeconds)
        : '';

    root._techTimeState.draft = parts
        ? { h: parts.h, m: parts.m, s: parts.s }
        : null;

    if (fire) {
        Tech.trigger(root, 'Change', {
            sender: visible,
            value: hidden.value
        });
    }
}

function techOpenTimePicker(root) {

    const hidden = root.querySelector('input[type="hidden"]');
    const input = root.querySelector('input.tech-input-control');

    if (!hidden || !input) return;

    document.querySelectorAll('.tech-timepicker.open')
        .forEach(function (other) {
            if (other !== root) techCloseTimePicker(other);
        });

    root._techTimeState.draft = techParseTimeValue(hidden.value);

    techRenderTimePicker(root);

    root.classList.add('open');

    const panel = root.querySelector('.tech-timepicker-panel');

    if (panel) {
        techFloatPopup(root, panel, { anchor: root });
    }

    input.setAttribute('aria-expanded', 'true');
}

function techCloseTimePicker(root) {

    techReleasePopup(root);

    root.classList.remove('open');

    const input = root.querySelector('input.tech-input-control');

    if (input) input.setAttribute('aria-expanded', 'false');
}

function initTimePicker(root) {

    const input = root.querySelector('input.tech-input-control');
    const hidden = root.querySelector('input[type="hidden"]');
    const panel = root.querySelector('.tech-timepicker-panel');

    if (!input || !hidden || !panel || input._techTimePicker) return;

    input._techTimePicker = true;

    root._techTimeState = { draft: null };

    if (hidden.value) {

        techSetTimePickerValue(
            root, techParseTimeValue(hidden.value), false);
    }

    function open() {

        if (input.disabled) return;

        techOpenTimePicker(root);
    }

    input.addEventListener('focus', function () {

        if (root.dataset.openOnFocus === 'false') return;

        open();
    });

    input.addEventListener('click', open);

    const toggle = root.querySelector('.tech-timepicker-toggle');

    if (toggle) {
        toggle.addEventListener('click', function () {

            if (root.classList.contains('open')) {
                techCloseTimePicker(root);
            }
            else {
                open();
            }
        });
    }

    panel.addEventListener('click', function (ev) {

        ev.stopPropagation();

        const state = root._techTimeState;

        const min = techParseTimeValue(root.dataset.min);
        const max = techParseTimeValue(root.dataset.max);
        const withSeconds = root.dataset.seconds === 'true';

        const hourButton = ev.target.closest('[data-h]');

        if (hourButton) {

            const base = state.draft || { h: 0, m: 0, s: 0 };

            state.draft = techClampTime(
                { h: +hourButton.dataset.h, m: base.m, s: base.s },
                min,
                max);

            techRenderTimePicker(root);

            return;
        }

        const minuteButton = ev.target.closest('[data-m]');

        if (minuteButton) {

            const base = state.draft || { h: 0, m: 0, s: 0 };

            state.draft = techClampTime(
                { h: base.h, m: +minuteButton.dataset.m, s: base.s },
                min,
                max);

            if (withSeconds) {
                techRenderTimePicker(root);
            }
            else {
                techSetTimePickerValue(root, state.draft, true);
                techCloseTimePicker(root);
            }

            return;
        }

        const secondButton = ev.target.closest('[data-s]');

        if (secondButton) {

            const base = state.draft || { h: 0, m: 0, s: 0 };

            state.draft = techClampTime(
                { h: base.h, m: base.m, s: +secondButton.dataset.s },
                min,
                max);

            techSetTimePickerValue(root, state.draft, true);
            techCloseTimePicker(root);

            return;
        }

        const periodButton = ev.target.closest('[data-period]');

        if (periodButton) {

            const draft = state.draft
                ? { h: state.draft.h, m: state.draft.m, s: state.draft.s }
                : { h: 0, m: 0, s: 0 };

            if (periodButton.dataset.period === 'am' && draft.h >= 12) {
                draft.h -= 12;
            }
            else if (periodButton.dataset.period === 'pm' && draft.h < 12) {
                draft.h += 12;
            }

            state.draft = techClampTime(draft, min, max);

            techRenderTimePicker(root);

            return;
        }

        const action = ev.target.closest('[data-action]');

        if (!action) return;

        if (action.dataset.action === 'now') {

            const now = new Date();

            const parts = techClampTime(
                { h: now.getHours(), m: now.getMinutes(), s: 0 },
                min,
                max);

            techSetTimePickerValue(root, parts, true);
        }
        else {
            techSetTimePickerValue(root, null, true);
        }

        techCloseTimePicker(root);
    });

    Tech.TimePicker.instances.set(input.id, {
        element: root,
        value: function () { return hidden.value; },
        setValue: function (text) {
            techSetTimePickerValue(root, techParseTimeValue(text), true);
        },
        open: open,
        close: function () { techCloseTimePicker(root); }
    });
}

Tech.register('timepicker', initTimePicker);

document.addEventListener('keydown', function (ev) {

    if (ev.key !== 'Escape') return;

    let handled = false;

    document.querySelectorAll('.tech-dropdown-wrapper.open')
        .forEach(function (wrapper) {

            const select = wrapper.querySelector('select');

            if (select) {
                closeDropdown(select);
                handled = true;
            }
        });

    document.querySelectorAll('.tech-datepicker.open')
        .forEach(function (root) {
            techCloseDatePicker(root);
            handled = true;
        });

    document.querySelectorAll('.tech-daterange.open')
        .forEach(function (root) {
            techCloseDateRange(root);
            handled = true;
        });

    document.querySelectorAll('.tech-timepicker.open')
        .forEach(function (root) {
            techCloseTimePicker(root);
            handled = true;
        });

    document.querySelectorAll('.tech-multiselect.open')
        .forEach(function (root) {

            if (root._techMultiSelect) {
                root._techMultiSelect.close();
                handled = true;
            }
        });

    // First Escape closes the popup; only the next one should
    // reach Bootstrap and dismiss an open modal.
    if (handled) {
        ev.preventDefault();
        ev.stopPropagation();
    }
}, true);

// ============================================================
// Editor
// ============================================================

Tech.Editor = Tech.Editor || {};
Tech.Editor.instances = Tech.Editor.instances || new Map();

Tech.Editor.get = function (id) {
    return Tech.Editor.instances.get(id) || null;
};

Tech.Editor.value = function (id) {

    const instance = Tech.Editor.get(id);

    return instance ? instance.value() : null;
};

Tech.Editor.setValue = function (id, html) {

    const instance = Tech.Editor.get(id);

    if (instance) instance.setValue(html);
};

function initEditor(root) {

    const body = root.querySelector('.tech-editor-body');
    const hidden = root.querySelector('input[type="hidden"]');

    if (!body || !hidden || body._techEditor) return;

    body._techEditor = true;

    const toolbar = root.querySelector('.tech-editor-toolbar');
    const counter = root.querySelector('.tech-input-counter');
    const max = parseInt(root.dataset.maxlength || '0', 10);
    const readOnly = body.getAttribute('contenteditable') === 'false';

    const imageUploadUrl = root.dataset.imageUpload || '';
    const fileUploadUrl = root.dataset.fileUpload || '';
    const libraryUrl = root.dataset.mediaLibrary || '';
    const uploadField = root.dataset.uploadField || 'file';
    const maxUpload = parseInt(root.dataset.maxUploadSize || '0', 10);

    let lastValue = hidden.value || '';
    let savedRange = null;

    if (hidden.value) body.innerHTML = hidden.value;

    function isEmpty() {

        if (body.querySelector('img.tech-editor-image, a.tech-editor-file')) {
            return false;
        }

        return body.innerText.trim() === '';
    }

    function sync() {

        hidden.value = isEmpty() ? '' : body.innerHTML;

        if (counter) {

            const length = body.textContent.length;

            counter.textContent = max
                ? length + '/' + max
                : String(length);
        }
    }

    function commit() {

        sync();

        if (hidden.value !== lastValue) {

            lastValue = hidden.value;

            Tech.trigger(root, 'Change', {
                sender: body,
                value: hidden.value
            });
        }
    }

    function saveSelection() {

        const selection = window.getSelection();

        if (!selection || selection.rangeCount === 0) return;

        const range = selection.getRangeAt(0);

        if (body.contains(range.commonAncestorContainer)) {
            savedRange = range.cloneRange();
        }
    }

    function insertAtCaret(node) {

        body.focus();

        const selection = window.getSelection();
        let range = null;

        if (selection && selection.rangeCount > 0 &&
            body.contains(selection.anchorNode)) {

            range = selection.getRangeAt(0);
        }
        else if (savedRange && body.contains(savedRange.commonAncestorContainer)) {

            range = savedRange;
        }

        if (!range) {
            body.appendChild(node);
            return;
        }

        range.deleteContents();
        range.insertNode(node);

        range.setStartAfter(node);
        range.collapse(true);

        if (selection) {

            selection.removeAllRanges();
            selection.addRange(range);
        }
    }

    function insertMedia(kind, media) {

        if (kind === 'image') {

            const image = document.createElement('img');

            image.src = media.url;
            image.alt = media.name || '';
            image.loading = 'lazy';
            image.className = 'tech-editor-image';

            insertAtCaret(image);
        }
        else {

            const link = document.createElement('a');

            link.href = media.url;
            link.className = 'tech-editor-file';
            link.target = '_blank';
            link.rel = 'noopener';
            link.download = media.name || '';

            const icon = document.createElement('i');

            icon.className = 'bi bi-paperclip';

            const label = document.createElement('span');

            label.textContent = media.name || 'فایل';

            link.appendChild(icon);
            link.appendChild(document.createTextNode(' '));
            link.appendChild(label);

            insertAtCaret(link);
        }

        commit();
    }

    function formatSize(bytes) {

        if (!bytes) return '';

        if (bytes >= 1024 * 1024) {
            return (bytes / (1024 * 1024))
                .toFixed(1)
                .replace('.0', '') + ' MB';
        }

        if (bytes >= 1024) return Math.round(bytes / 1024) + ' KB';

        return bytes + ' B';
    }

    function isSafeUrl(url) {

        return typeof url === 'string' &&
            /^(https?:\/\/|\/)/i.test(url);
    }

    function normalizeMedia(payload) {

        if (!payload) return null;

        const item = payload.data || payload.item || payload;

        const media = typeof item === 'string'
            ? { url: item, name: '' }
            : {
                url: item.url,
                name: item.name || item.title || '',
                size: item.size || 0,
                type: item.type || ''
            };

        return isSafeUrl(media.url) ? media : null;
    }

    async function uploadMedia(file, kind) {

        const url = kind === 'image' ? imageUploadUrl : fileUploadUrl;

        if (!url) throw new Error('آدرس آپلود تنظیم نشده است.');

        if (maxUpload > 0 && file.size > maxUpload) {
            throw new Error(
                'حجم فایل بیشتر از ' + formatSize(maxUpload) + ' است.');
        }

        if (kind === 'image' && file.type &&
            file.type.indexOf('image/') !== 0) {
            throw new Error('فقط فایل تصویری مجاز است.');
        }

        const data = new FormData();

        data.append(uploadField, file, file.name);
        data.append('kind', kind);

        const headers = { 'Accept': 'application/json' };
        const token = Tech.getAntiforgeryToken(root);

        if (token) {

            data.append('__RequestVerificationToken', token);
            headers['RequestVerificationToken'] = token;
        }

        const response = await fetch(url, {
            method: 'POST',
            body: data,
            headers: headers,
            credentials: 'same-origin'
        });

        if (!response.ok) {

            let message = '';

            try {
                const error = await response.json();
                message = error.message || error.title || '';
            }
            catch {
                message = '';
            }

            throw new Error(message || 'آپلود انجام نشد.');
        }

        return normalizeMedia(await response.json());
    }

    function buildLibrary(picker, kind, select) {

        const section = document.createElement('div');
        const grid = document.createElement('div');
        const title = document.createElement('div');

        section.className = 'tech-media-library';
        title.className = 'tech-media-library-title';
        title.textContent = 'فروشگاه فایل‌ها';

        grid.className = 'tech-media-grid';

        section.appendChild(title);
        section.appendChild(grid);
        picker.appendChild(section);

        grid.innerHTML = '<span class="tech-media-empty">در حال بارگذاری…</span>';

        Tech.requestJson(libraryUrl, {
            params: { type: kind === 'image' ? 'image' : 'file' }
        }).then(function (result) {

            const list = Array.isArray(result)
                ? result
                : (result && (result.items || result.data)) || [];

            const items = list.map(normalizeMedia).filter(Boolean);

            grid.innerHTML = '';

            if (items.length === 0) {

                grid.innerHTML =
                    '<span class="tech-media-empty">فایلی یافت نشد.</span>';

                return;
            }

            items.forEach(function (media) {

                const item = document.createElement('button');

                item.type = 'button';
                item.className = 'tech-media-item';
                item.title = media.name || media.url;

                if (kind === 'image') {

                    const image = document.createElement('img');

                    image.src = media.url;
                    image.alt = media.name || '';
                    image.loading = 'lazy';

                    item.appendChild(image);
                }
                else {

                    const icon = document.createElement('i');

                    icon.className = 'bi bi-file-earmark-text';

                    item.appendChild(icon);
                }

                const name = document.createElement('span');

                name.className = 'tech-media-name';
                name.textContent = media.name || media.url;

                item.appendChild(name);

                if (media.size) {

                    const size = document.createElement('span');

                    size.className = 'tech-media-size';
                    size.textContent = formatSize(media.size);

                    item.appendChild(size);
                }

                item.addEventListener('click', function () {
                    select(media);
                });

                grid.appendChild(item);
            });
        })
        .catch(function () {

            grid.innerHTML =
                '<span class="tech-media-empty">دریافت فایل‌ها ممکن نشد.</span>';
        });
    }

    function buildPicker(picker, kind, select) {

        const isImage = kind === 'image';
        const uploadUrl = isImage ? imageUploadUrl : fileUploadUrl;

        if (uploadUrl) {

            const upload = document.createElement('div');
            const hint = isImage
                ? 'PNG، JPG، GIF یا WEBP' +
                    (maxUpload > 0 ? ' تا ' + formatSize(maxUpload) : '')
                : (maxUpload > 0
                    ? 'حداکثر ' + formatSize(maxUpload)
                    : 'همه انواع فایل');

            upload.className = 'tech-media-upload';
            upload.innerHTML =
                '<div class="tech-media-dropzone" tabindex="0" role="button">' +
                    '<i class="bi bi-cloud-arrow-up tech-media-drop-icon"></i>' +
                    '<span class="tech-media-drop-title">' +
                        'فایل را اینجا رها کنید یا برای انتخاب کلیک کنید' +
                    '</span>' +
                    '<span class="tech-media-hint">' +
                        Tech.escapeHtml(hint) +
                    '</span>' +
                    '<input type="file" hidden />' +
                '</div>' +
                '<div class="tech-media-status" role="status" aria-live="polite"></div>';

            picker.appendChild(upload);

            const dropzone = upload.querySelector('.tech-media-dropzone');
            const input = upload.querySelector('input[type="file"]');
            const status = upload.querySelector('.tech-media-status');

            if (isImage) input.accept = 'image/png,image/jpeg,image/gif,image/webp';

            function setStatus(message, isError) {

                status.textContent = message || '';
                status.classList.toggle('is-error', !!isError);
            }

            async function handle(file) {

                if (!file) return;

                if (maxUpload > 0 && file.size > maxUpload) {

                    setStatus(
                        'حجم فایل بیشتر از ' + formatSize(maxUpload) + ' است.',
                        true);

                    return;
                }

                if (isImage && file.type &&
                    file.type.indexOf('image/') !== 0) {

                    setStatus('فقط فایل تصویری مجاز است.', true);

                    return;
                }

                dropzone.classList.add('is-busy');
                input.disabled = true;
                setStatus('در حال آپلود…', false);

                try {

                    const media = await uploadMedia(file, kind);

                    if (!media) throw new Error('پاسخ سرور معتبر نیست.');

                    select(media);
                }
                catch (error) {

                    dropzone.classList.remove('is-busy');
                    input.disabled = false;
                    setStatus(error.message || 'آپلود انجام نشد.', true);
                }
            }

            dropzone.addEventListener('click', function () {
                input.click();
            });

            dropzone.addEventListener('keydown', function (ev) {

                if (ev.key === 'Enter' || ev.key === ' ') {

                    ev.preventDefault();
                    input.click();
                }
            });

            input.addEventListener('change', function () {

                const file = input.files && input.files[0];

                input.value = '';

                handle(file);
            });

            ['dragenter', 'dragover'].forEach(function (name) {

                dropzone.addEventListener(name, function (ev) {

                    ev.preventDefault();
                    dropzone.classList.add('is-dragover');
                });
            });

            ['dragleave', 'dragend'].forEach(function (name) {

                dropzone.addEventListener(name, function () {
                    dropzone.classList.remove('is-dragover');
                });
            });

            dropzone.addEventListener('drop', function (ev) {

                ev.preventDefault();
                dropzone.classList.remove('is-dragover');

                const file = ev.dataTransfer &&
                    ev.dataTransfer.files &&
                    ev.dataTransfer.files[0];

                handle(file);
            });
        }

        if (libraryUrl) {
            buildLibrary(picker, kind, select);
        }
    }

    function openMediaPicker(kind) {

        saveSelection();

        const isImage = kind === 'image';
        const hasSource = isImage
            ? (imageUploadUrl || libraryUrl)
            : (fileUploadUrl || libraryUrl);

        if (!hasSource) return;

        let chosen = null;

        Tech.dialog({
            title: isImage ? 'درج تصویر' : 'درج فایل',
            type: 'info',
            size: 'lg',
            closable: true,
            content: '<div class="tech-media-picker" data-kind="' +
                kind + '"></div>',
            buttons: [
                {
                    text: 'انصراف',
                    value: false,
                    variant: 'outline-secondary'
                }
            ],
            onOpen: function (context) {

                const picker = context.dialog.querySelector(
                    '.tech-media-picker');

                if (!picker) return;

                buildPicker(picker, kind, function (media) {

                    chosen = media;

                    context.close(true);
                });
            }
        }).then(function () {

            if (chosen) insertMedia(kind, chosen);
        });
    }

    sync();

    if (toolbar && !readOnly) {

        toolbar.querySelectorAll('[data-cmd]')
            .forEach(function (button) {

                button.addEventListener('mousedown', function (ev) {
                    ev.preventDefault();
                    saveSelection();
                });

                button.addEventListener('click', function () {

                    const command = button.dataset.cmd;

                    if (command === 'insertImage' ||
                        command === 'insertFile') {

                        openMediaPicker(
                            command === 'insertImage' ? 'image' : 'file');

                        return;
                    }

                    if (document.activeElement !== body) {
                        body.focus();
                    }

                    if (command === 'createLink') {

                        const url = window.prompt(
                            'آدرس لینک را وارد کنید:', 'https://');

                        if (!url) return;

                        document.execCommand('createLink', false, url);
                    }
                    else {
                        document.execCommand(command, false, null);
                    }

                    sync();
                });
            });

        const stateButtons = Array.from(
            toolbar.querySelectorAll('[data-cmd]'))
            .filter(function (button) {
                return button.dataset.cmd !== 'createLink' &&
                    button.dataset.cmd !== 'removeFormat' &&
                    button.dataset.cmd !== 'insertImage' &&
                    button.dataset.cmd !== 'insertFile';
            });

        document.addEventListener('selectionchange', function () {

            if (!root.classList.contains('tech-editor-focused')) return;

            stateButtons.forEach(function (button) {

                let active = false;

                try {
                    active = document.queryCommandState(button.dataset.cmd);
                }
                catch {
                    active = false;
                }

                button.classList.toggle('active', active);
            });
        });
    }

    body.addEventListener('focus', function () {
        root.classList.add('tech-editor-focused');
    });

    body.addEventListener('blur', function () {

        root.classList.remove('tech-editor-focused');

        if (isEmpty()) body.innerHTML = '';

        sync();

        if (hidden.value !== lastValue) {

            lastValue = hidden.value;

            Tech.trigger(root, 'Change', {
                sender: body,
                value: hidden.value
            });
        }
    });

    body.addEventListener('input', sync);

    body.addEventListener('paste', function (ev) {

        if (readOnly) return;

        const clipboard = ev.clipboardData || window.clipboardData;

        if (!clipboard) return;

        ev.preventDefault();

        document.execCommand(
            'insertText',
            false,
            clipboard.getData('text/plain'));
    });

    Tech.Editor.instances.set(body.id, {
        element: root,
        body: body,
        value: function () { return hidden.value; },
        setValue: function (html) {

            body.innerHTML = html || '';

            sync();

            lastValue = hidden.value;

            Tech.trigger(root, 'Change', {
                sender: body,
                value: hidden.value
            });
        }
    });
}

Tech.register('editor', initEditor);

// ============================================================
// Choice controls (checkbox / radio group)
// ============================================================

function initChoice(root) {

    const inputs = Array.from(
        root.querySelectorAll('input.tech-choice-input'));

    if (inputs.length === 0 || root._techChoice) return;

    root._techChoice = true;

    if (root.dataset.readonly === 'true') {

        inputs.forEach(function (input) {

            input.addEventListener('click', function (ev) {
                ev.preventDefault();
            });

            input.addEventListener('keydown', function (ev) {

                if (ev.key === ' ' || ev.key === 'Enter') {
                    ev.preventDefault();
                }
            });
        });
    }

    inputs.forEach(function (input) {

        input.addEventListener('change', function () {

            if (input.type === 'radio' && !input.checked) return;

            Tech.trigger(root, 'Change', {
                sender: input,
                checked: input.checked,
                value: input.type === 'checkbox'
                    ? (input.checked ? input.value : 'false')
                    : input.value
            });
        });
    });
}

Tech.register('checkbox', initChoice);
Tech.register('radiogroup', initChoice);

// ============================================================
// Tabs
// ============================================================

Tech.Tabs = Tech.Tabs || {};
Tech.Tabs.instances = Tech.Tabs.instances || new Map();

Tech.Tabs.get = function (id) {
    return Tech.Tabs.instances.get(id) || null;
};

Tech.Tabs.select = function (id, index) {

    const instance = Tech.Tabs.get(id);

    if (instance) instance.select(index);
};

function techTabsSelect(root, index, fire) {

    const tabs = Array.from(root.querySelectorAll('.tech-tab'));
    const panels = Array.from(root.querySelectorAll('.tech-tab-panel'));

    if (index < 0 || index >= tabs.length) return;

    tabs.forEach(function (tab, position) {

        const active = position === index;

        tab.classList.toggle('is-active', active);
        tab.setAttribute('aria-selected', active ? 'true' : 'false');
        tab.setAttribute('tabindex', active ? '0' : '-1');

        if (panels[position]) {
            panels[position].classList.toggle('is-active', active);
        }
    });

    if (fire) {
        Tech.trigger(root, 'Change', {
            sender: tabs[index],
            index: index,
            tab: tabs[index],
            panel: panels[index] || null
        });
    }
}

function initTabs(root) {

    const tabs = Array.from(root.querySelectorAll('.tech-tab'));

    if (tabs.length === 0 || root._techTabsReady) return;

    root._techTabsReady = true;

    tabs.forEach(function (tab, index) {

        tab.addEventListener('click', function () {

            if (tab.classList.contains('is-active')) return;

            techTabsSelect(root, index, true);
        });

        tab.addEventListener('keydown', function (ev) {

            let target = null;

            if (ev.key === 'ArrowRight') {
                target = index === 0 ? tabs.length - 1 : index - 1;
            }
            else if (ev.key === 'ArrowLeft') {
                target = (index + 1) % tabs.length;
            }
            else if (ev.key === 'Home') {
                target = 0;
            }
            else if (ev.key === 'End') {
                target = tabs.length - 1;
            }

            if (target === null) return;

            ev.preventDefault();

            tabs[target].focus();

            techTabsSelect(root, target, true);
        });
    });

    const instanceId = root.id ||
        'tech-tabs-' + Math.random().toString(36).slice(2, 10);

    Tech.Tabs.instances.set(instanceId, {
        element: root,
        select: function (index) { techTabsSelect(root, index, true); },
        active: function () {
            return tabs.findIndex(function (tab) {
                return tab.classList.contains('is-active');
            });
        }
    });
}

Tech.register('tabs', initTabs);

// ============================================================
// Wizard / Stepper
// ============================================================

Tech.Wizard = Tech.Wizard || {};
Tech.Wizard.instances = Tech.Wizard.instances || new Map();

Tech.Wizard.get = function (id) {

    if (!id) return null;

    if (typeof id !== 'string') {
        return id._techWizard || null;
    }

    return Tech.Wizard.instances.get(id) || null;
};

Tech.Wizard.goTo = function (id, index) {

    const wizard = Tech.Wizard.get(id);

    return wizard ? wizard.goTo(index) : false;
};

function initWizard(root) {

    if (!root || root._techWizard) return;

    const steps = Array.from(root.querySelectorAll('.tech-wizard-step'));
    const buttons = steps.map(function (step) {
        return step.querySelector('.tech-wizard-step-btn');
    });
    const panels = Array.from(root.querySelectorAll('.tech-wizard-panel'));
    const prevButton = root.querySelector('.tech-wizard-prev');
    const nextButton = root.querySelector('.tech-wizard-next');
    const finishButton = root.querySelector('.tech-wizard-finish');
    const progress = root.querySelector('.tech-wizard-progress');
    const progressBar = root.querySelector('.tech-wizard-progress-bar');

    if (steps.length === 0 || panels.length !== steps.length) return;

    const linear = root.dataset.linear !== 'false';
    const submitOnFinish = root.dataset.submitOnFinish === 'true';
    const start = Math.min(
        Math.max(parseInt(root.dataset.start || '0', 10) || 0, 0),
        steps.length - 1);

    const completed = new Set();

    let index = start;

    for (let position = 0; position < start; position++) {
        completed.add(position);
    }

    function setStatus(step, done, active) {

        const status = step.querySelector('.tech-wizard-sr');

        if (!status) return;

        status.textContent = done
            ? 'تکمیل‌شده'
            : active ? 'مرحلهٔ جاری' : '';
    }

    function state() {

        return {
            index: index,
            total: steps.length,
            last: index === steps.length - 1,
            completed: steps.map(function (step, position) {
                return completed.has(position);
            }),
            steps: steps.map(function (step, position) {
                const title = step.querySelector('.tech-wizard-title');

                return {
                    index: position,
                    title: title ? title.textContent : '',
                    active: position === index,
                    completed: completed.has(position)
                };
            })
        };
    }

    function render(options) {

        options = options || {};

        const total = steps.length;

        steps.forEach(function (step, position) {

            const active = position === index;
            const done = completed.has(position);
            const button = buttons[position];

            step.classList.toggle('is-active', active);
            step.classList.toggle('is-completed', done);

            if (active) step.classList.remove('has-error');

            if (button) {

                if (active) {
                    button.setAttribute('aria-current', 'step');
                }
                else {
                    button.removeAttribute('aria-current');
                }

                button.disabled = linear && position > index;
                button.setAttribute('tabindex', active ? '0' : '-1');
            }

            panels[position].hidden = !active;
            panels[position].classList.toggle('is-active', active);

            setStatus(step, done, active);
        });

        const reachedEnd = completed.size === total && index === total - 1;

        const percent = total === 0
            ? 0
            : reachedEnd ? 100 : Math.round(index * 100 / total);

        if (progressBar) progressBar.style.width = percent + '%';

        if (progress) {
            progress.setAttribute('aria-valuenow', String(percent));
            progress.setAttribute('aria-valuetext',
                'مرحله ' + (index + 1) + ' از ' + total);
        }

        if (prevButton) prevButton.hidden = index === 0;
        if (nextButton) nextButton.hidden = index === total - 1;
        if (finishButton) finishButton.hidden = index !== total - 1;

        if (options.focus) {

            try {
                panels[index].focus({ preventScroll: true });
            }
            catch {
                panels[index].focus();
            }
        }

        if (options.fire) {

            Tech.trigger(root, 'Change', {
                index: index,
                from: options.from,
                step: steps[index],
                panel: panels[index],
                state: state()
            });
        }
    }

    function validateCurrent() {

        const valid = Tech.validate(panels[index]);

        steps[index].classList.toggle('has-error', !valid);

        if (!valid) {
            root.dispatchEvent(new CustomEvent('tech:wizard-invalid', {
                bubbles: true,
                detail: { index: index }
            }));
        }

        return valid;
    }

    function submitForm() {

        if (!submitOnFinish) return;

        const form = root.closest('form');

        if (!form) return;

        if (typeof form.requestSubmit === 'function') {
            form.requestSubmit();
        }
        else {
            form.submit();
        }
    }

    function finish() {

        if (!validateCurrent()) return false;

        completed.add(index);
        render({});

        const snapshot = state();

        Tech.trigger(root, 'Complete', snapshot);
        Tech.trigger(root, 'Finish', snapshot);

        submitForm();

        return true;
    }

    function goTo(target, options) {

        if (target === null || target === undefined) return false;

        const position = Math.min(
            Math.max(parseInt(target, 10) || 0, 0),
            steps.length - 1);

        if (position === index) {
            if (options && options.focus) render({ focus: true });
            return true;
        }

        if (linear && position > index) return false;

        const from = index;

        index = position;

        render({ fire: true, from: from, focus: true });

        return true;
    }

    function next() {

        if (index === steps.length - 1) return finish();
        if (!validateCurrent()) return false;

        completed.add(index);

        const from = index;

        index++;

        render({ fire: true, from: from, focus: true });

        return true;
    }

    function previous() {

        if (index === 0) return false;

        const from = index;

        index--;

        render({ fire: true, from: from, focus: true });

        return true;
    }

    function reset() {

        completed.clear();

        for (let position = 0; position < start; position++) {
            completed.add(position);
        }

        index = start;

        steps.forEach(function (step) {
            step.classList.remove('has-error');
        });

        render({});
    }

    buttons.forEach(function (button, position) {

        if (!button) return;

        button.addEventListener('click', function () {
            goTo(position, { focus: true });
        });
    });

    if (prevButton) {
        prevButton.addEventListener('click', previous);
    }

    if (nextButton) {
        nextButton.addEventListener('click', next);
    }

    if (finishButton) {
        finishButton.addEventListener('click', finish);
    }

    function clearError(event) {

        const panel = event.target.closest
            ? event.target.closest('.tech-wizard-panel')
            : null;

        if (!panel) return;

        const position = panels.indexOf(panel);

        if (position >= 0) {
            steps[position].classList.remove('has-error');
        }
    }

    root.addEventListener('input', clearError);
    root.addEventListener('change', clearError);

    const instanceId = root.id ||
        'tech-wizard-' + Math.random().toString(36).slice(2, 10);

    root._techWizard = {
        element: root,
        next: next,
        prev: previous,
        previous: previous,
        goTo: goTo,
        finish: finish,
        reset: reset,
        current: function () { return index; },
        state: state
    };

    Tech.Wizard.instances.set(instanceId, root._techWizard);

    render({});
}

Tech.register('wizard', initWizard);

// ============================================================
// Accordion
// ============================================================

Tech.Accordion = Tech.Accordion || {};
Tech.Accordion.instances = Tech.Accordion.instances || new Map();

Tech.Accordion.get = function (id) {
    return Tech.Accordion.instances.get(id) || null;
};

function techAccordionSet(item, open) {

    item.classList.toggle('is-open', open);

    const header = item.querySelector('.tech-accordion-header');

    if (header) {
        header.setAttribute('aria-expanded', open ? 'true' : 'false');
    }
}

function initAccordion(root) {

    const items = Array.from(
        root.querySelectorAll('.tech-accordion-item'));

    if (items.length === 0 || root._techAccordionReady) return;

    root._techAccordionReady = true;

    const multiple = root.dataset.multiple === 'true';

    items.forEach(function (item, index) {

        const header = item.querySelector('.tech-accordion-header');

        if (!header) return;

        header.addEventListener('click', function () {

            const open = !item.classList.contains('is-open');

            if (open && !multiple) {
                items.forEach(function (other) {
                    if (other !== item) techAccordionSet(other, false);
                });
            }

            techAccordionSet(item, open);

            Tech.trigger(root, 'Change', {
                sender: header,
                index: index,
                open: open
            });
        });
    });

    const instanceId = root.id ||
        'tech-accordion-' + Math.random().toString(36).slice(2, 10);

    Tech.Accordion.instances.set(instanceId, {
        element: root,
        toggle: function (index) {
            const item = items[index];
            if (item) item.querySelector('.tech-accordion-header').click();
        },
        open: function (index) {
            const item = items[index];
            if (item) techAccordionSet(item, true);
        },
        close: function (index) {
            const item = items[index];
            if (item) techAccordionSet(item, false);
        }
    });
}

Tech.register('accordion', initAccordion);

// ============================================================
// TreeView
// ============================================================

Tech.TreeView = Tech.TreeView || {};
Tech.TreeView.instances = Tech.TreeView.instances || new Map();

Tech.TreeView.get = function (id) {
    return Tech.TreeView.instances.get(id) || null;
};

function techTreeLookup(data, path) {

    const clean = String(path || '').trim().replace(/^data\./, '');

    if (!clean) return '';

    let value = data;

    for (const part of clean.split('.')) {

        if (value === null || value === undefined) return '';

        value = value[part];
    }

    return value === null || value === undefined ? '' : value;
}

function techTreeRenderTemplate(template, data) {

    return String(template || '').replace(
        /#([:=])([\s\S]*?)#/g,
        function (match, mode, expression) {

            const value = techTreeLookup(data, expression);

            return mode === ':'
                ? Tech.escapeHtml(value)
                : String(value);
        });
}

function initTreeView(root) {

    const list = root.querySelector('.tech-treeview-root');

    if (!list || root._techTreeReady) return;

    root._techTreeReady = true;

    const nodes = Array.from(
        root.querySelectorAll('.tech-tree-node'));

    if (nodes.length === 0) return;

    const selection = root.dataset.selection || 'single';
    const multi = selection === 'multiple';
    const hasCheckboxes = root.dataset.checkboxes === 'true';
    const template = root.dataset.clientTemplate || '';

    function payloadOf(node) {

        if (!node._techTreeData) {

            try {
                node._techTreeData = JSON.parse(node.dataset.node || '{}');
            }
            catch {
                node._techTreeData = {};
            }
        }

        return node._techTreeData;
    }

    function rowOf(node) {
        return node.querySelector(':scope > .tech-tree-row');
    }

    function labelOf(node) {
        return node.querySelector(
            ':scope > .tech-tree-row > .tech-tree-label');
    }

    function childrenOf(node) {
        return node.querySelector(':scope > .tech-tree-children');
    }

    function toggleOf(node) {
        return node.querySelector(
            ':scope > .tech-tree-row > .tech-tree-toggle');
    }

    function checkboxOf(node) {
        return node.querySelector(
            ':scope > .tech-tree-row > .tech-tree-checkbox');
    }

    function directChildren(node) {

        const children = childrenOf(node);

        return children
            ? Array.from(children.children)
                .filter(function (child) {
                    return child.classList.contains('tech-tree-node');
                })
            : [];
    }

    function parentOf(node) {

        const parent = node.parentElement;

        return parent
            ? parent.closest('.tech-tree-node')
            : null;
    }

    function isRtl(node) {

        const row = rowOf(node);

        return row
            ? getComputedStyle(row).direction === 'rtl'
            : false;
    }

    function isDisabled(node) {
        return node.classList.contains('is-disabled');
    }

    function rows() {

        return nodes
            .map(rowOf)
            .filter(function (row) {
                return row !== null && row.offsetParent !== null;
            });
    }

    function setActiveRow(row) {

        nodes.forEach(function (node) {

            const current = rowOf(node);

            if (current) current.tabIndex = current === row ? 0 : -1;
        });
    }

    function setExpanded(node, expanded) {

        if (!childrenOf(node)) return;

        node.classList.toggle('is-expanded', expanded);
        node.setAttribute('aria-expanded', expanded ? 'true' : 'false');

        const toggle = toggleOf(node);

        if (toggle) {
            toggle.setAttribute(
                'aria-label',
                expanded ? 'بستن' : 'باز کردن');
        }
    }

    function isExpanded(node) {
        return node.classList.contains('is-expanded');
    }

    function selectedIds() {

        return nodes
            .filter(function (node) {
                return node.classList.contains('is-selected');
            })
            .map(function (node) {
                return payloadOf(node).id;
            });
    }

    function checkedIds() {

        return nodes
            .filter(function (node) {
                const checkbox = checkboxOf(node);
                return checkbox && checkbox.checked;
            })
            .map(function (node) {
                return payloadOf(node).id;
            });
    }

    function selectNode(node, additive) {

        if (selection === 'none' || isDisabled(node)) return;

        if (!multi || !additive) {

            nodes.forEach(function (other) {

                other.classList.remove('is-selected');
                other.setAttribute('aria-selected', 'false');
            });
        }

        const wasSelected = node.classList.contains('is-selected');

        if (multi && additive && wasSelected) {

            node.classList.remove('is-selected');
            node.setAttribute('aria-selected', 'false');
        }
        else {

            node.classList.add('is-selected');
            node.setAttribute('aria-selected', 'true');
        }

        const data = payloadOf(node);

        Tech.trigger(root, 'Select', {
            sender: rowOf(node),
            id: data.id,
            text: data.text,
            selectedIds: selectedIds()
        });
    }

    function setSubtreeChecked(node, checked) {

        const checkbox = checkboxOf(node);

        if (checkbox && !checkbox.disabled) {
            checkbox.checked = checked;
            checkbox.indeterminate = false;
        }

        directChildren(node).forEach(function (child) {
            setSubtreeChecked(child, checked);
        });
    }

    function refreshAncestors(node) {

        let parent = parentOf(node);

        while (parent) {

            const box = checkboxOf(parent);

            if (box) {

                const boxes = directChildren(parent)
                    .map(checkboxOf)
                    .filter(Boolean);

                if (boxes.length > 0) {

                    const all = boxes.every(function (childBox) {
                        return childBox.checked && !childBox.indeterminate;
                    });

                    const any = boxes.some(function (childBox) {
                        return childBox.checked || childBox.indeterminate;
                    });

                    box.checked = all;
                    box.indeterminate = !all && any;
                }
            }

            parent = parentOf(parent);
        }
    }

    function refreshCheckboxStates() {

        nodes.slice().reverse().forEach(function (node) {

            const box = checkboxOf(node);

            if (!box) return;

            const boxes = directChildren(node)
                .map(checkboxOf)
                .filter(Boolean);

            if (boxes.length === 0) return;

            const all = boxes.every(function (childBox) {
                return childBox.checked && !childBox.indeterminate;
            });

            const any = boxes.some(function (childBox) {
                return childBox.checked || childBox.indeterminate;
            });

            if (all) {
                box.checked = true;
                box.indeterminate = false;
            }
            else {
                box.checked = false;
                box.indeterminate = any;
            }
        });
    }

    if (template) {

        nodes.forEach(function (node) {

            const label = labelOf(node);

            if (!label) return;

            try {
                label.innerHTML = techTreeRenderTemplate(
                    template,
                    payloadOf(node));
            }
            catch {
                // Keep the server-rendered label on template errors.
            }
        });
    }

    nodes.forEach(function (node) {

        const row = rowOf(node);
        const toggle = toggleOf(node);
        const checkbox = checkboxOf(node);

        if (!row) return;

        row.addEventListener('click', function (ev) {

            if (ev.target.closest(
                '.tech-tree-toggle, .tech-tree-checkbox')) {

                return;
            }

            row.focus();

            selectNode(
                node,
                ev.ctrlKey || ev.metaKey);
        });

        row.addEventListener('focus', function () {
            setActiveRow(row);
        });

        if (toggle) {

            toggle.addEventListener('click', function (ev) {

                ev.stopPropagation();

                if (isDisabled(node)) return;

                setExpanded(node, !isExpanded(node));
            });
        }

        if (checkbox) {

            checkbox.addEventListener('change', function (ev) {

                ev.stopPropagation();

                setSubtreeChecked(node, checkbox.checked);
                refreshAncestors(node);

                const data = payloadOf(node);

                Tech.trigger(root, 'Check', {
                    sender: checkbox,
                    id: data.id,
                    checked: checkedIds()
                });
            });
        }

        row.addEventListener('keydown', function (ev) {

            const visible = rows();
            const index = visible.indexOf(row);

            let target = -1;

            if (ev.key === 'ArrowDown') {
                target = Math.min(index + 1, visible.length - 1);
            }
            else if (ev.key === 'ArrowUp') {
                target = Math.max(index - 1, 0);
            }
            else if (ev.key === 'Home') {
                target = 0;
            }
            else if (ev.key === 'End') {
                target = visible.length - 1;
            }
            else if (ev.key === 'Enter' || ev.key === ' ') {

                ev.preventDefault();

                selectNode(node, ev.ctrlKey || ev.metaKey);

                return;
            }
            else if (ev.key === 'ArrowRight' || ev.key === 'ArrowLeft') {

                if (isDisabled(node)) return;

                const rtl = isRtl(node);
                const expandKey = rtl ? 'ArrowLeft' : 'ArrowRight';

                if (ev.key === expandKey) {

                    if (childrenOf(node) && !isExpanded(node)) {
                        setExpanded(node, true);
                    }
                }
                else if (isExpanded(node)) {
                    setExpanded(node, false);
                }

                ev.preventDefault();

                return;
            }

            if (target < 0 || target >= visible.length) return;

            ev.preventDefault();

            visible[target].focus();
        });
    });

    if (hasCheckboxes) {
        refreshCheckboxStates();
    }

    if (root.dataset.expandAll === 'true') {

        nodes.forEach(function (node) {
            setExpanded(node, true);
        });
    }

    const firstRow = rows()[0];

    if (firstRow) setActiveRow(firstRow);

    const byId = new Map();

    nodes.forEach(function (node) {
        byId.set(payloadOf(node).id, node);
    });

    const instanceId = root.id ||
        'tech-treeview-' + Math.random().toString(36).slice(2, 10);

    Tech.TreeView.instances.set(instanceId, {
        element: root,
        expandAll: function () {
            nodes.forEach(function (node) {
                setExpanded(node, true);
            });
        },
        collapseAll: function () {
            nodes.forEach(function (node) {
                setExpanded(node, false);
            });
        },
        expand: function (id) {
            const node = byId.get(id);
            if (node) setExpanded(node, true);
        },
        collapse: function (id) {
            const node = byId.get(id);
            if (node) setExpanded(node, false);
        },
        select: function (id) {

            const node = byId.get(id);

            if (!node) return;

            const row = rowOf(node);

            if (row) row.focus();

            selectNode(node, false);
        },
        selectedIds: selectedIds,
        checkedIds: checkedIds,
        getNode: function (id) {

            const node = byId.get(id);

            return node ? payloadOf(node) : null;
        }
    });
}

Tech.register('treeview', initTreeView);


// ============================================================
// DropDownTree
// ============================================================

Tech.DropDownTree = Tech.DropDownTree || {};
Tech.DropDownTree.instances = Tech.DropDownTree.instances || new Map();

Tech.DropDownTree.get = function (id) {
    return Tech.DropDownTree.instances.get(id) || null;
};

function initDropDownTree(root) {

    if (!root || root._techDropDownTree) return;

    const control = root.querySelector('.tech-droptree-control');
    const panel = root.querySelector('.tech-droptree-panel');
    const treeHost = root.querySelector('.tech-droptree-tree');
    const valuesHost = root.querySelector('.tech-droptree-values-host');
    const displayHost = root.querySelector('.tech-droptree-values');
    const search = root.querySelector('.tech-droptree-search');
    const emptyHost = root.querySelector('.tech-droptree-empty');

    if (!control || !panel || !treeHost || !valuesHost || !displayHost) {
        return;
    }

    const name = root.dataset.name || '';
    const multiple = root.dataset.selection === 'multiple';
    const hasCheckboxes = root.dataset.checkboxes === 'true';
    const filterable = root.dataset.filterable === 'true';
    const closeOnSelect = root.dataset.closeOnSelect === 'true';
    const placeholder = root.dataset.placeholder || '';
    const template = root.dataset.clientTemplate || '';
    const disabled = root.classList.contains('tech-droptree-disabled');
    const readOnly = root.classList.contains('tech-droptree-readonly');

    const nodes = Array.from(treeHost.querySelectorAll('.tech-tree-node'));
    const valueInput = valuesHost.querySelector('input[type="hidden"]');

    let isOpen = false;
    let activeRow = null;
    let filterTerm = '';
    let savedExpansion = null;

    function payloadOf(node) {

        if (!node._techDropTreeData) {

            try {
                node._techDropTreeData = JSON.parse(node.dataset.node || '{}');
            }
            catch {
                node._techDropTreeData = {};
            }
        }

        return node._techDropTreeData;
    }

    function rowOf(node) {
        return node.querySelector(':scope > .tech-tree-row');
    }

    function labelOf(node) {
        return node.querySelector(
            ':scope > .tech-tree-row > .tech-tree-label');
    }

    function childrenOf(node) {
        return node.querySelector(':scope > .tech-tree-children');
    }

    function toggleOf(node) {
        return node.querySelector(
            ':scope > .tech-tree-row > .tech-tree-toggle');
    }

    function checkboxOf(node) {
        return node.querySelector(
            ':scope > .tech-tree-row > .tech-tree-checkbox');
    }

    function directChildren(node) {

        const children = childrenOf(node);

        return children
            ? Array.from(children.children)
                .filter(function (child) {
                    return child.classList.contains('tech-tree-node');
                })
            : [];
    }

    function parentOf(node) {

        const parent = node.parentElement;

        return parent ? parent.closest('.tech-tree-node') : null;
    }

    function isRtl(node) {

        const row = rowOf(node);

        return row
            ? getComputedStyle(row).direction === 'rtl'
            : false;
    }

    function isDisabled(node) {
        return node.classList.contains('is-disabled');
    }

    function valueOf(node) {
        return String(payloadOf(node).value);
    }

    function nodeByKey(key) {

        const text = String(key);

        return nodes.find(function (node) {
            const data = payloadOf(node);

            return String(data.id) === text ||
                String(data.value) === text;
        }) || null;
    }

    function checkedNodes() {

        if (hasCheckboxes) {

            return nodes.filter(function (node) {
                const box = checkboxOf(node);
                return box && box.checked;
            });
        }

        return nodes.filter(function (node) {
            return node.classList.contains('is-selected');
        });
    }

    function currentValues() {

        return checkedNodes().map(valueOf);
    }

    function currentTexts() {

        return checkedNodes().map(function (node) {
            return payloadOf(node).text || '';
        });
    }

    function currentIds() {

        return checkedNodes().map(function (node) {
            return payloadOf(node).id;
        });
    }

    function visibleRows() {

        return nodes
            .map(rowOf)
            .filter(function (row) {
                return row && row.offsetParent !== null;
            });
    }

    function setExpanded(node, expanded) {

        if (!childrenOf(node)) return;

        node.classList.toggle('is-expanded', expanded);
        node.setAttribute('aria-expanded', expanded ? 'true' : 'false');

        const toggle = toggleOf(node);

        if (toggle) {
            toggle.setAttribute('aria-label', expanded ? 'بستن' : 'باز کردن');
        }
    }

    function setActive(row) {

        activeRow = row;

        nodes.forEach(function (node) {

            const current = rowOf(node);

            if (current) current.tabIndex = current === row ? 0 : -1;
        });
    }

    function focusRow(row) {

        if (!row) return;

        setActive(row);
        row.focus();
    }

    function focusInitialRow() {

        const selected = checkedNodes()[0] || nodes[0];

        focusRow(selected ? rowOf(selected) : visibleRows()[0]);
    }

    function setSubtreeChecked(node, checked) {

        const box = checkboxOf(node);

        if (box && !box.disabled) {
            box.checked = checked;
            box.indeterminate = false;
        }

        directChildren(node).forEach(function (child) {
            setSubtreeChecked(child, checked);
        });
    }

    function refreshAncestors(node) {

        let parent = parentOf(node);

        while (parent) {

            const box = checkboxOf(parent);

            if (box) {

                const boxes = directChildren(parent)
                    .map(checkboxOf)
                    .filter(Boolean);

                if (boxes.length > 0) {

                    const all = boxes.every(function (childBox) {
                        return childBox.checked && !childBox.indeterminate;
                    });

                    const any = boxes.some(function (childBox) {
                        return childBox.checked || childBox.indeterminate;
                    });

                    box.checked = all;
                    box.indeterminate = !all && any;
                }
            }

            parent = parentOf(parent);
        }
    }

    function refreshCheckboxStates() {

        nodes.slice().reverse().forEach(function (node) {

            const box = checkboxOf(node);

            if (!box) return;

            const boxes = directChildren(node)
                .map(checkboxOf)
                .filter(Boolean);

            if (boxes.length === 0) return;

            const all = boxes.every(function (childBox) {
                return childBox.checked && !childBox.indeterminate;
            });

            const any = boxes.some(function (childBox) {
                return childBox.checked || childBox.indeterminate;
            });

            box.checked = all;
            box.indeterminate = !all && any;
        });
    }

    function syncSelectionClasses() {

        if (!hasCheckboxes) return;

        nodes.forEach(function (node) {

            const box = checkboxOf(node);
            const selected = !!box && box.checked;

            node.classList.toggle('is-selected', selected);
            node.setAttribute('aria-selected', selected ? 'true' : 'false');
        });
    }

    function ensureClearButton() {

        if (!root.dataset.clearable || root.dataset.clearable !== 'true') {
            return null;
        }

        let button = control.querySelector('.tech-droptree-clear');

        if (button) return button;

        button = document.createElement('button');
        button.type = 'button';
        button.tabIndex = -1;
        button.setAttribute('aria-label', 'پاک کردن');
        button.className = 'tech-droptree-clear';
        button.textContent = '×';

        const arrow = control.querySelector('.tech-droptree-arrow');

        if (arrow) control.insertBefore(button, arrow);
        else control.appendChild(button);

        button.addEventListener('click', onClearClick);

        return button;
    }

    function renderDisplay() {

        const entries = checkedNodes();

        displayHost.innerHTML = '';

        if (entries.length === 0) {

            const empty = document.createElement('span');

            empty.className = 'tech-droptree-placeholder';
            empty.textContent = placeholder;
            displayHost.appendChild(empty);
        }
        else if (multiple) {

            entries.forEach(function (node) {

                const data = payloadOf(node);
                const chip = document.createElement('span');

                chip.className = 'tech-droptree-chip';
                chip.dataset.value = valueOf(node);

                const text = document.createElement('span');

                text.className = 'tech-droptree-chip-text';
                text.textContent = data.text || '';

                chip.appendChild(text);

                if (!disabled && !readOnly && root.dataset.clearable === 'true') {

                    const remove = document.createElement('button');

                    remove.type = 'button';
                    remove.tabIndex = -1;
                    remove.setAttribute('aria-label', 'حذف');
                    remove.className = 'tech-droptree-chip-remove';
                    remove.textContent = '×';

                    chip.appendChild(remove);
                }

                displayHost.appendChild(chip);
            });
        }
        else {

            const text = document.createElement('span');

            text.className = 'tech-droptree-text';
            text.textContent = payloadOf(entries[0]).text || '';
            displayHost.appendChild(text);
        }

        const clear = ensureClearButton();

        if (clear) clear.hidden = entries.length === 0;
    }

    function syncHidden(emitChange) {

        if (!valueInput) return;

        const values = currentValues();

        Array.from(valuesHost.querySelectorAll('input[type="hidden"]'))
            .forEach(function (input) {

                if (input !== valueInput) input.remove();
            });

        valueInput.value = values.length > 0 ? values[0] : '';

        for (let index = 1; index < values.length; index++) {

            const hidden = document.createElement('input');

            hidden.type = 'hidden';
            hidden.name = name;
            hidden.value = values[index];

            valuesHost.appendChild(hidden);
        }

        if (emitChange) {
            valueInput.dispatchEvent(new Event('change', { bubbles: true }));
        }
    }

    function sync(emitChange) {

        syncSelectionClasses();
        renderDisplay();
        syncHidden(emitChange);

        if (!emitChange) return;

        const values = currentValues();

        Tech.trigger(root, 'Change', {
            sender: root,
            values: values,
            texts: currentTexts(),
            value: multiple ? null : (values[0] || null)
        });
    }

    function fireSelect(node) {

        const data = payloadOf(node);

        Tech.trigger(root, 'Select', {
            sender: rowOf(node),
            id: data.id,
            text: data.text,
            value: data.value,
            values: currentValues(),
            selectedIds: currentIds()
        });
    }

    function fireCheck(node, sender) {

        Tech.trigger(root, 'Check', {
            sender: sender || checkboxOf(node),
            id: payloadOf(node).id,
            checked: currentIds()
        });
    }

    function clearSelection(emitChange) {

        nodes.forEach(function (node) {

            node.classList.remove('is-selected');
            node.setAttribute('aria-selected', 'false');

            const box = checkboxOf(node);

            if (box) {
                box.checked = false;
                box.indeterminate = false;
            }
        });

        sync(emitChange);

        if (emitChange) {
            Tech.trigger(root, 'Select', {
                sender: root,
                id: null,
                text: null,
                value: null,
                values: [],
                selectedIds: []
            });
        }
    }

    function selectNode(node, additive) {

        if (!node || isDisabled(node)) return;

        if (hasCheckboxes) {

            const box = checkboxOf(node);

            if (!box || box.disabled) return;

            setSubtreeChecked(node, !box.checked);
            refreshAncestors(node);
            sync(true);
            fireSelect(node);
            fireCheck(node, box);
        }
        else if (multiple) {

            if (!additive) {

                nodes.forEach(function (other) {

                    if (other !== node) {
                        other.classList.remove('is-selected');
                        other.setAttribute('aria-selected', 'false');
                    }
                });
            }

            const selected = node.classList.contains('is-selected');

            node.classList.toggle('is-selected', !selected);
            node.setAttribute('aria-selected', selected ? 'false' : 'true');

            sync(true);
            fireSelect(node);
        }
        else {

            nodes.forEach(function (other) {

                other.classList.toggle('is-selected', other === node);
                other.setAttribute(
                    'aria-selected',
                    other === node ? 'true' : 'false');
            });

            sync(true);
            fireSelect(node);

            if (closeOnSelect) close(true);
        }
    }

    function uncheckByValue(value) {

        const node = nodes.find(function (item) {
            return valueOf(item) === String(value);
        });

        if (!node) return;

        if (hasCheckboxes) {

            setSubtreeChecked(node, false);
            refreshAncestors(node);
            sync(true);
            fireCheck(node);
        }
        else {

            node.classList.remove('is-selected');
            node.setAttribute('aria-selected', 'false');
            sync(true);
        }
    }

    function saveExpansion() {

        savedExpansion = nodes.map(function (node) {
            return node.classList.contains('is-expanded');
        });
    }

    function restoreExpansion() {

        if (!savedExpansion) return;

        nodes.forEach(function (node, index) {
            setExpanded(node, savedExpansion[index]);
        });

        savedExpansion = null;
    }

    function applyFilter(term) {

        const text = String(term || '').trim().toLowerCase();

        if (!text) {

            if (filterTerm) restoreExpansion();

            filterTerm = '';

            nodes.forEach(function (node) {
                node.classList.remove('is-filter-hidden');
            });

            if (emptyHost) emptyHost.hidden = nodes.length > 0;

            return;
        }

        if (!filterTerm) saveExpansion();

        filterTerm = text;

        let anyVisible = false;

        function walk(node) {

            const own = String(payloadOf(node).text || '')
                .toLowerCase()
                .includes(text);

            let childMatch = false;

            directChildren(node).forEach(function (child) {

                if (walk(child)) childMatch = true;
            });

            const show = own || childMatch;

            node.classList.toggle('is-filter-hidden', !show);

            if (show) {

                anyVisible = true;

                if (childMatch) setExpanded(node, true);
            }

            return show;
        }

        nodes.forEach(function (node) {

            if (!parentOf(node)) walk(node);
        });

        if (emptyHost) emptyHost.hidden = anyVisible;
    }

    function onDocumentDown(ev) {

        if (!root.contains(ev.target)) close(false);
    }

    function onClearClick(ev) {

        ev.preventDefault();
        ev.stopPropagation();

        clearSelection(true);
        control.focus();
    }

    function open() {

        if (isOpen || disabled || readOnly) return;

        document.querySelectorAll('.tech-droptree.open')
            .forEach(function (other) {

                if (other !== root && other._techDropDownTree) {
                    other._techDropDownTree.close(false);
                }
            });

        isOpen = true;

        root.classList.add('open');
        control.setAttribute('aria-expanded', 'true');

        techFloatPopup(root, panel, {
            anchor: control,
            width: 'anchor'
        });

        document.addEventListener('mousedown', onDocumentDown, true);

        if (search) {

            search.value = '';
            applyFilter('');
            setTimeout(function () { search.focus(); }, 30);
        }
        else {
            focusInitialRow();
        }

        Tech.trigger(root, 'Open', { sender: root });
    }

    function close(focusControl) {

        if (!isOpen) return;

        isOpen = false;

        root.classList.remove('open');
        control.setAttribute('aria-expanded', 'false');

        techReleasePopup(root);

        document.removeEventListener('mousedown', onDocumentDown, true);

        if (filterable && filterTerm) {

            if (search) search.value = '';
            applyFilter('');
        }

        if (focusControl) control.focus();

        Tech.trigger(root, 'Close', { sender: root });
    }

    if (template) {

        nodes.forEach(function (node) {

            const label = labelOf(node);

            if (!label) return;

            try {
                label.innerHTML = techTreeRenderTemplate(
                    template,
                    payloadOf(node));
            }
            catch {
                // Keep the server-rendered label on template errors.
            }
        });
    }

    if (hasCheckboxes) {

        nodes
            .filter(function (node) {
                const box = checkboxOf(node);
                return box && box.checked;
            })
            .forEach(function (node) {
                setSubtreeChecked(node, true);
            });

        refreshCheckboxStates();
    }

    nodes.forEach(function (node) {

        const row = rowOf(node);
        const toggle = toggleOf(node);
        const box = checkboxOf(node);

        if (!row) return;

        row.addEventListener('click', function (ev) {

            if (ev.target.closest(
                '.tech-tree-toggle, .tech-tree-checkbox')) {
                return;
            }

            row.focus();

            selectNode(node, ev.ctrlKey || ev.metaKey);
        });

        row.addEventListener('focus', function () {
            setActive(row);
        });

        if (toggle) {

            toggle.addEventListener('click', function (ev) {

                ev.stopPropagation();

                if (isDisabled(node)) return;

                setExpanded(
                    node,
                    !node.classList.contains('is-expanded'));
            });
        }

        if (box) {

            box.addEventListener('change', function (ev) {

                ev.stopPropagation();

                setSubtreeChecked(node, box.checked);
                refreshAncestors(node);

                sync(true);
                fireSelect(node);
                fireCheck(node, box);
            });
        }

        row.addEventListener('keydown', function (ev) {

            const visible = visibleRows();
            const index = visible.indexOf(row);

            let target = -1;

            if (ev.key === 'ArrowDown') {
                target = Math.min(index + 1, visible.length - 1);
            }
            else if (ev.key === 'ArrowUp') {
                target = Math.max(index - 1, 0);
            }
            else if (ev.key === 'Home') {
                target = 0;
            }
            else if (ev.key === 'End') {
                target = visible.length - 1;
            }
            else if (ev.key === 'Enter' || ev.key === ' ') {

                ev.preventDefault();

                selectNode(node, ev.ctrlKey || ev.metaKey);

                return;
            }
            else if (ev.key === 'Escape') {

                ev.preventDefault();
                close(true);

                return;
            }
            else if (ev.key === 'Tab') {

                close(false);
                return;
            }
            else if (ev.key === 'ArrowRight' || ev.key === 'ArrowLeft') {

                if (isDisabled(node)) return;

                const expandKey = isRtl(node) ? 'ArrowLeft' : 'ArrowRight';

                if (ev.key === expandKey) {

                    if (childrenOf(node) &&
                        !node.classList.contains('is-expanded')) {

                        setExpanded(node, true);
                    }
                }
                else if (node.classList.contains('is-expanded')) {
                    setExpanded(node, false);
                }

                ev.preventDefault();

                return;
            }

            if (target < 0 || target >= visible.length) return;

            ev.preventDefault();

            focusRow(visible[target]);
        });
    });

    control.addEventListener('click', function (ev) {

        if (ev.target.closest(
            '.tech-droptree-clear, .tech-droptree-chip-remove')) {
            return;
        }

        if (disabled || readOnly) return;

        if (isOpen) close(false);
        else open();
    });

    control.addEventListener('keydown', function (ev) {

        if (ev.key === 'Enter' || ev.key === ' ' || ev.key === 'ArrowDown') {

            ev.preventDefault();
            open();

            if (!filterable) focusInitialRow();

            return;
        }

        if (ev.key === 'Escape') {

            close(false);
            return;
        }

        if ((ev.key === 'Backspace' || ev.key === 'Delete') &&
            !multiple &&
            root.dataset.clearable === 'true') {

            ev.preventDefault();
            clearSelection(true);
        }
    });

    displayHost.addEventListener('click', function (ev) {

        const remove = ev.target.closest('.tech-droptree-chip-remove');

        if (!remove) return;

        ev.preventDefault();
        ev.stopPropagation();

        const chip = remove.closest('.tech-droptree-chip');

        if (chip) uncheckByValue(chip.dataset.value);
    });

    const initialClear = control.querySelector('.tech-droptree-clear');

    if (initialClear) {
        initialClear.addEventListener('click', onClearClick);
    }

    if (search) {

        search.addEventListener('input', function () {
            applyFilter(search.value);
        });

        search.addEventListener('keydown', function (ev) {

            if (ev.key === 'Escape') {

                ev.preventDefault();
                close(true);
            }
            else if (ev.key === 'ArrowDown') {

                ev.preventDefault();
                focusRow(visibleRows()[0]);
            }
            else if (ev.key === 'Enter') {

                ev.preventDefault();
            }
        });
    }

    syncSelectionClasses();
    renderDisplay();
    syncHidden(false);

    const firstRow = visibleRows()[0];

    if (firstRow) setActive(firstRow);

    const instanceId = root.id ||
        'tech-droptree-' + Math.random().toString(36).slice(2, 10);

    const api = {
        element: root,
        open: open,
        close: function () { close(false); },
        value: function () {
            const values = currentValues();
            return multiple ? values : (values[0] || '');
        },
        values: currentValues,
        texts: currentTexts,
        selectedIds: currentIds,
        select: function (key) {

            const node = nodeByKey(key);

            if (node) selectNode(node, multiple);
        },
        clear: function () { clearSelection(true); },
        getNode: function (key) {

            const node = nodeByKey(key);

            return node ? payloadOf(node) : null;
        }
    };

    root._techDropDownTree = api;
    Tech.DropDownTree.instances.set(instanceId, api);
}

Tech.register('droptree', initDropDownTree);



// ============================================================
// Spinner
// ============================================================

Tech.Spinner = Tech.Spinner || {};

Tech.Spinner.overlay = function (target, options) {

    target = typeof target === 'string'
        ? document.querySelector(target)
        : target;

    if (!target) return null;

    options = options || {};

    const size = String(options.size || 'md')
        .replace(/[^a-z0-9-]/gi, '')
        .toLowerCase();

    const color = String(options.color || 'primary')
        .replace(/[^a-z0-9-]/gi, '');

    const overlay = document.createElement('div');

    overlay.className = 'tech-spinner-overlay is-visible';

    overlay.innerHTML = '<span class="tech-spinner tech-spinner-' + size
        + ' tech-spinner-' + color + '" role="status">'
        + '<span class="tech-spinner-circle"></span>'
        + (options.label
            ? '<span class="tech-spinner-label">'
                + Tech.escapeHtml(options.label) + '</span>'
            : '')
        + '</span>';

    target.classList.add('tech-spinner-host');
    target.appendChild(overlay);

    return {
        element: overlay,
        show: function () { overlay.classList.add('is-visible'); },
        hide: function () { overlay.classList.remove('is-visible'); },
        remove: function () {
            overlay.remove();
            target.classList.remove('tech-spinner-host');
        }
    };
};

// ============================================================
// Form validation
// ============================================================

const techValidationDefaults = {
    required: 'وارد کردن {0} الزامی است.',
    length: 'طول {0} باید بین {2} و {1} کاراکتر باشد.',
    minlength: 'طول {0} باید حداقل {1} کاراکتر باشد.',
    maxlength: 'طول {0} نباید بیشتر از {1} کاراکتر باشد.',
    range: 'مقدار {0} باید بین {1} و {2} باشد.',
    email: 'فرمت {0} معتبر نیست.',
    regex: 'فرمت {0} معتبر نیست.',
    phone: 'فرمت {0} معتبر نیست.',
    url: 'فرمت {0} معتبر نیست.',
    creditcard: 'شماره کارت {0} معتبر نیست.',
    equalto: '{0} با {1} مطابقت ندارد.'
};

function techToAsciiDigits(value) {

    return String(value === null || value === undefined ? '' : value)
        .replace(/[۰-۹]/g, function (c) {
            return String('۰۱۲۳۴۵۶۷۸۹'.indexOf(c));
        })
        .replace(/[٠-٩]/g, function (c) {
            return String('٠١٢٣٤٥٦٧٨٩'.indexOf(c));
        });
}

function techParseNumber(value) {

    const text = techToAsciiDigits(value).replace(/[,\s\u066c\u200f\u200e]/g, '');

    if (!text) return null;

    const number = parseFloat(text);

    return isNaN(number) ? null : number;
}

function techIsControl(el) {

    return !!el && el.matches &&
        el.matches('input, select, textarea');
}

function techValidationRoot(el) {

    return el.closest(
        '.tech-input, .tech-dropdown-wrapper, .tech-choice, .tech-droptree');
}

function techValidationControl(el) {

    if (techIsControl(el)) return el;

    return el.querySelector('input[name], select[name], textarea[name]');
}

function techValidationName(el) {

    if (el.name) return el.name;

    const control = techValidationControl(el);

    return control ? control.name : '';
}

function techValidationValue(el) {

    if (el.classList && el.classList.contains('tech-choice')) {

        if (el.classList.contains('tech-choice-radio')) {

            const checked = el.querySelector('input[name]:checked');

            return checked ? checked.value : '';
        }

        const box = el.querySelector('input[type="checkbox"]');

        return box && box.checked ? (box.value || 'true') : '';
    }

    const control = techIsControl(el) ? el : techValidationControl(el);

    if (!control) return '';

    if (control.type === 'checkbox' || control.type === 'radio') {
        return control.checked ? control.value || 'true' : '';
    }

    return control.value || '';
}

function techValidationMessage(template, replacements, fallback) {

    const pattern = template || fallback || '';

    return String(pattern).replace(/\{(\d+)\}/g, function (match, index) {

        const value = replacements[Number(index)];

        return value === undefined || value === null ? match : String(value);
    });
}

function techValidationRule(el) {

    const data = el.dataset || {};
    const label = data.valName || techValidationName(el);
    const value = String(techValidationValue(el)).trim();

    const required = el.hasAttribute('data-val-required');

    if (required && !value) {
        return techValidationMessage(
            data.valRequired, [label], techValidationDefaults.required);
    }

    if (!value) return null;

    if (el.hasAttribute('data-val-length')) {

        const min = parseInt(data.valLengthMin || '0', 10) || 0;
        const max = parseInt(data.valLengthMax || '0', 10) || 0;

        if ((min > 0 && value.length < min) ||
            (max > 0 && value.length > max)) {

            return techValidationMessage(
                data.valLength,
                [label, max, min],
                techValidationDefaults.length);
        }
    }

    if (el.hasAttribute('data-val-minlength')) {

        const min = parseInt(data.valMinlengthMin || '0', 10) || 0;

        if (min > 0 && value.length < min) {
            return techValidationMessage(
                data.valMinlength,
                [label, min],
                techValidationDefaults.minlength);
        }
    }

    if (el.hasAttribute('data-val-maxlength')) {

        const max = parseInt(data.valMaxlengthMax || '0', 10) || 0;

        if (max > 0 && value.length > max) {
            return techValidationMessage(
                data.valMaxlength,
                [label, max],
                techValidationDefaults.maxlength);
        }
    }

    if (el.hasAttribute('data-val-range')) {

        const normalized = value.toLowerCase();
        let invalid = false;

        if (normalized === 'true' || normalized === 'false') {

            const min = String(data.valRangeMin || '').toLowerCase();
            const max = String(data.valRangeMax || '').toLowerCase();

            invalid = (min === 'true' && normalized === 'false') ||
                (max === 'false' && normalized === 'true');
        }
        else {

            const number = techParseNumber(value);
            const min = techParseNumber(data.valRangeMin);
            const max = techParseNumber(data.valRangeMax);

            invalid = number === null ||
                (min !== null && number < min) ||
                (max !== null && number > max);
        }

        if (invalid) {

            return techValidationMessage(
                data.valRange,
                [label, data.valRangeMin, data.valRangeMax],
                techValidationDefaults.range);
        }
    }

    if (el.hasAttribute('data-val-email') &&
        !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value)) {

        return techValidationMessage(
            data.valEmail, [label], techValidationDefaults.email);
    }

    if (el.hasAttribute('data-val-phone') &&
        !/^[+()\d\s-]{7,}$/.test(value)) {

        return techValidationMessage(
            data.valPhone, [label], techValidationDefaults.phone);
    }

    if (el.hasAttribute('data-val-url') &&
        !/^(https?:\/\/|www\.)\S+$/i.test(value)) {

        return techValidationMessage(
            data.valUrl, [label], techValidationDefaults.url);
    }

    if (el.hasAttribute('data-val-creditcard')) {

        const digits = techToAsciiDigits(value).replace(/\D/g, '');

        let sum = 0;
        let double = false;

        for (let i = digits.length - 1; i >= 0; i--) {

            let digit = parseInt(digits.charAt(i), 10);

            if (double) {
                digit *= 2;
                if (digit > 9) digit -= 9;
            }

            sum += digit;
            double = !double;
        }

        if (digits.length < 13 || digits.length > 19 || sum % 10 !== 0) {
            return techValidationMessage(
                data.valCreditcard,
                [label],
                techValidationDefaults.creditcard);
        }
    }

    if (el.hasAttribute('data-val-regex')) {

        const pattern = data.valRegexPattern;

        if (pattern) {

            let regex = null;

            try {
                regex = new RegExp(pattern);
            }
            catch {
                regex = null;
            }

            if (regex && !regex.test(value)) {
                return techValidationMessage(
                    data.valRegex, [label], techValidationDefaults.regex);
            }
        }
    }

    if (el.hasAttribute('data-val-equalto')) {

        const other = String(data.valEqualtoOther || '')
            .replace(/^\*\./, '');

        if (other) {

            const form = el.closest('form') || document;
            const target = form.querySelector(
                '[name="' + other.replace(/"/g, '\\"') + '"]');

            if (target && techValidationValue(target) !== value) {
                return techValidationMessage(
                    data.valEqualto,
                    [label, other],
                    techValidationDefaults.equalto);
            }
        }
    }

    return null;
}

function techValidationSpan(root, name) {

    if (!name) return null;

    let span = root ? root.nextElementSibling : null;

    if (span && span.matches && span.matches('[data-valmsg-for]')) {
        return span;
    }

    const form = root ? root.closest('form') : null;
    const scope = form || document;

    return Array.from(scope.querySelectorAll('[data-valmsg-for]'))
        .find(function (item) {
            return item.getAttribute('data-valmsg-for') === name;
        }) || null;
}

function techValidationApply(el, message) {

    const root = techValidationRoot(el);
    const control = techIsControl(el) ? el : techValidationControl(el);
    const name = techValidationName(el);

    if (root) {
        root.classList.toggle('tech-input-invalid', !!message);
    }

    if (el.classList) {
        el.classList.toggle('is-invalid', !!message);
    }

    if (control && control !== el) {
        control.classList.toggle('is-invalid', !!message);
    }

    if (root && root.classList.contains('tech-choice')) {

        root.querySelectorAll('input').forEach(function (input) {
            input.classList.toggle('is-invalid', !!message);
        });
    }

    if (control) {
        control.setAttribute('aria-invalid', message ? 'true' : 'false');
    }

    if (root && root.classList.contains('tech-droptree')) {

        const combobox = root.querySelector('.tech-droptree-control');

        if (combobox) {
            combobox.setAttribute(
                'aria-invalid',
                message ? 'true' : 'false');
        }
    }

    const span = techValidationSpan(root || el, name);

    if (span) {
        span.textContent = message || '';
    }
}

function techValidateField(el) {

    if (!el || el.dataset.val !== 'true') return null;

    const message = techValidationRule(el);

    techValidationApply(el, message);

    return message;
}

Tech.validateField = function (element) {

    return !techValidateField(element);
};

Tech.validate = function (form) {

    if (!form) return true;

    let valid = true;
    let first = null;

    form.querySelectorAll('[data-val="true"]').forEach(function (el) {

        const message = techValidateField(el);

        if (message) {

            valid = false;

            if (!first) first = el;
        }
    });

    if (first) {

        const control = techIsControl(first)
            ? first
            : techValidationControl(first) || first;

        if (control.scrollIntoView) {
            control.scrollIntoView({ block: 'center', behavior: 'smooth' });
        }

        if (control.focus) {
            control.focus({ preventScroll: true });
        }
    }

    return valid;
};

function techWireValidation(root) {

    const forms = root.querySelectorAll
        ? Array.from(root.querySelectorAll('form'))
        : [];

    if (root.matches && root.matches('form')) forms.push(root);

    forms.forEach(function (form) {

        if (form._techValidation) return;

        const fields = form.querySelectorAll('[data-val="true"]');

        if (fields.length === 0) return;

        form._techValidation = true;
        form.setAttribute('novalidate', 'novalidate');

        form.addEventListener('submit', function (ev) {

            if (!Tech.validate(form)) {
                ev.preventDefault();
                ev.stopPropagation();
            }
        });

        const revalidate = function (target) {

            const el = target.closest
                ? target.closest('[data-val="true"]')
                : null;

            if (el) {
                techValidateField(el);
                return;
            }

            const fieldRoot = target.closest
                ? target.closest('.tech-input-invalid')
                : null;

            if (!fieldRoot) return;

            fieldRoot.querySelectorAll('[data-val="true"]')
                .forEach(techValidateField);
        };

        form.addEventListener('input', function (ev) {
            revalidate(ev.target);
        });

        form.addEventListener('change', function (ev) {
            revalidate(ev.target);
        });
    });
}

Tech.wireValidation = techWireValidation;

// ============================================================
// MultiSelect / Tag input
// ============================================================

function initMultiSelect(root) {

    if (!root || root._techMultiSelect) return;

    const input = root.querySelector('.tech-multiselect-input');
    const panel = root.querySelector('.tech-multiselect-panel');
    const optionsHost = root.querySelector('.tech-multiselect-options');
    const tagsHost = root.querySelector('.tech-multiselect-tags');
    const valuesHost = root.querySelector('.tech-multiselect-values');
    const emptyHost = root.querySelector('.tech-multiselect-empty');
    const controlHost = root.querySelector('.tech-multiselect-control');

    if (!input || !panel || !optionsHost ||
        !tagsHost || !valuesHost) return;

    const disabled = root.classList.contains('tech-multiselect-disabled');
    const serverFiltering = root.dataset.serverFiltering === 'true';
    const allowCustom = root.dataset.allowCustom === 'true';
    const maxItems = parseInt(root.dataset.maxItems || '0', 10) || 0;
    const textField = root.dataset.textField || 'text';
    const valueField = root.dataset.valueField || 'value';
    const placeholder = root.dataset.placeholder || '';

    let options = [];
    let selected = [];
    let isOpen = false;
    let activeIndex = -1;
    let loaded = false;
    let requestId = 0;
    let timer = null;

    function hasSource() {

        return !!(root.dataset.url || root.dataset.dataFunction);
    }

    function readDomOptions() {

        return Array.from(
            optionsHost.querySelectorAll('.tech-multiselect-option'))
            .map(function (button) {

                const label = button.querySelector(
                    '.tech-multiselect-option-text');

                return {
                    value: String(button.dataset.value || ''),
                    text: label ? label.textContent : button.textContent
                };
            })
            .filter(function (item) { return item.value; });
    }

    function createChip(item) {

        const chip = document.createElement('span');

        chip.className = 'tech-multiselect-chip';
        chip.dataset.value = item.value;

        const label = document.createElement('span');

        label.className = 'tech-multiselect-chip-text';
        label.textContent = item.text;

        chip.appendChild(label);

        if (!disabled) {

            const remove = document.createElement('button');

            remove.type = 'button';
            remove.tabIndex = -1;
            remove.className = 'tech-multiselect-chip-remove';
            remove.setAttribute('aria-label', 'حذف');
            remove.textContent = '×';

            chip.appendChild(remove);
        }

        return chip;
    }

    function sync() {

        tagsHost.innerHTML = '';

        selected.forEach(function (item) {
            tagsHost.appendChild(createChip(item));
        });

        valuesHost.innerHTML = '';

        selected.forEach(function (item) {

            const hidden = document.createElement('input');

            hidden.type = 'hidden';
            hidden.name = root.dataset.name;
            hidden.value = item.value;

            valuesHost.appendChild(hidden);
        });

        root.classList.toggle('has-value', selected.length > 0);

        input.placeholder = selected.length > 0 ? '' : placeholder;

        Array.from(
            optionsHost.querySelectorAll('.tech-multiselect-option'))
            .forEach(function (button) {

                const on = selected.some(function (item) {
                    return item.value === String(button.dataset.value);
                });

                button.classList.toggle('is-selected', on);
                button.setAttribute('aria-selected', on ? 'true' : 'false');
            });
    }

    function renderOptions(term) {

        const query = (term || '').trim().toLowerCase();

        const list = query && !serverFiltering
            ? options.filter(function (item) {

                return item.text.toLowerCase().indexOf(query) >= 0 ||
                    item.value.toLowerCase().indexOf(query) >= 0;
            })
            : options;

        optionsHost.innerHTML = '';
        activeIndex = -1;

        list.forEach(function (item) {

            const button = document.createElement('button');

            button.type = 'button';
            button.className = 'tech-multiselect-option';
            button.setAttribute('role', 'option');
            button.dataset.value = item.value;

            const on = selected.some(function (current) {
                return current.value === item.value;
            });

            button.classList.toggle('is-selected', on);
            button.setAttribute('aria-selected', on ? 'true' : 'false');

            const check = document.createElement('i');

            check.className = 'bi bi-check2 tech-multiselect-check';

            const label = document.createElement('span');

            label.className = 'tech-multiselect-option-text';
            label.textContent = item.text;

            button.appendChild(check);
            button.appendChild(label);

            button.addEventListener('click', function (ev) {

                ev.preventDefault();
                ev.stopPropagation();
                toggle(item);
            });

            optionsHost.appendChild(button);
        });

        if (emptyHost) emptyHost.hidden = list.length > 0;
    }

    function notify() {

        Tech.trigger(root, 'Change', {
            sender: root,
            values: selected.map(function (item) { return item.value; }),
            items: selected.map(function (item) {
                return { value: item.value, text: item.text };
            })
        });
    }

    function toggle(item) {

        const index = selected.findIndex(function (current) {
            return current.value === item.value;
        });

        if (index >= 0) {
            selected.splice(index, 1);
        }
        else {

            if (maxItems && selected.length >= maxItems) return;

            selected.push({ value: item.value, text: item.text });
        }

        sync();
        input.value = '';
        renderOptions('');
        notify();
    }

    function removeValue(value) {

        const index = selected.findIndex(function (item) {
            return item.value === value;
        });

        if (index < 0) return;

        selected.splice(index, 1);
        sync();
        renderOptions(input.value);
        notify();
    }

    function addCustom(text) {

        const value = String(text || '').trim();

        if (!value) return;

        if (selected.some(function (item) { return item.value === value; })) {
            input.value = '';
            return;
        }

        if (maxItems && selected.length >= maxItems) return;

        const match = options.find(function (item) {
            return item.value === value;
        });

        selected.push({ value: value, text: match ? match.text : value });

        input.value = '';
        sync();
        renderOptions('');
        notify();
    }

    function reconcile() {

        selected.forEach(function (item) {

            const match = options.find(function (option) {
                return option.value === item.value;
            });

            if (match) item.text = match.text;
        });
    }

    function mapData(data) {

        if (!Array.isArray(data)) return [];

        return data.map(function (item) {

            if (item === null || typeof item !== 'object') {

                const text = String(item);

                return { value: text, text: text };
            }

            const value = getProperty(item, valueField) !== undefined
                ? getProperty(item, valueField)
                : getProperty(item, 'value');

            const text = getProperty(item, textField) !== undefined
                ? getProperty(item, textField)
                : getProperty(item, 'text');

            return {
                value: String(value === undefined || value === null
                    ? ''
                    : value),
                text: String(text === undefined || text === null
                    ? value
                    : text)
            };
        }).filter(function (item) { return item.value; });
    }

    function loadOptions(term) {

        if (!hasSource()) {
            loaded = true;
            renderOptions(term);
            return;
        }

        const minLength = parseInt(root.dataset.minLength || '1', 10) || 1;

        if (serverFiltering && term && term.length < minLength) {
            options = [];
            renderOptions('');
            return;
        }

        const id = ++requestId;

        let promise;

        try {

            if (root.dataset.dataFunction) {

                promise = Promise.resolve(
                    window[root.dataset.dataFunction](term || ''));
            }
            else {

                promise = Tech.requestJson(root.dataset.url, {
                    method: root.dataset.method || 'GET',
                    params: { term: term || '' },
                    antiforgeryElement: root
                });
            }
        }
        catch (err) {

            console.error('TechUI multiselect:', err);

            options = [];
            renderOptions(term);

            return;
        }

        Promise.resolve(promise)
            .then(function (data) {

                if (id !== requestId) return;

                options = mapData(data);
                loaded = true;

                reconcile();
                sync();
                renderOptions(term);

                Tech.trigger(root, 'DataBound', {
                    sender: root,
                    data: data
                });
            })
            .catch(function (err) {

                if (id !== requestId) return;

                console.error('TechUI multiselect:', err);

                options = [];
                renderOptions(term);
            });
    }

    function onDocumentDown(ev) {

        if (!root.contains(ev.target)) closePanel();
    }

    function openPanel() {

        if (disabled || isOpen) return;

        isOpen = true;

        root.classList.add('open');
        input.setAttribute('aria-expanded', 'true');

        techFloatPopup(root, panel, { width: 'anchor' });

        if (!loaded) loadOptions('');
        else renderOptions(input.value);

        document.addEventListener('mousedown', onDocumentDown, true);

        Tech.trigger(root, 'Open', { sender: root });
    }

    function closePanel() {

        if (!isOpen) return;

        isOpen = false;

        root.classList.remove('open');
        input.setAttribute('aria-expanded', 'false');

        techReleasePopup(root);

        document.removeEventListener('mousedown', onDocumentDown, true);

        activeIndex = -1;

        Tech.trigger(root, 'Close', { sender: root });
    }

    options = readDomOptions();
    loaded = options.length > 0 || !hasSource();

    selected = Array.from(valuesHost.querySelectorAll('input[type="hidden"]'))
        .map(function (el) {

            const value = String(el.value || '');

            const match = options.find(function (item) {
                return item.value === value;
            });

            return { value: value, text: match ? match.text : value };
        })
        .filter(function (item) { return item.value; });

    sync();

    if (disabled) {

        root._techMultiSelect = {
            getValues: function () {
                return selected.map(function (item) { return item.value; });
            },
            open: function () { },
            close: function () { }
        };

        return;
    }

    if (controlHost) {

        controlHost.addEventListener('click', function (ev) {

            if (ev.target.closest('.tech-multiselect-chip-remove')) return;

            input.focus();
            openPanel();
        });
    }

    tagsHost.addEventListener('click', function (ev) {

        const button = ev.target.closest('.tech-multiselect-chip-remove');

        if (!button) return;

        ev.preventDefault();
        ev.stopPropagation();

        const chip = button.closest('.tech-multiselect-chip');

        if (chip) removeValue(chip.dataset.value);
    });

    input.addEventListener('focus', openPanel);

    input.addEventListener('input', function () {

        openPanel();

        if (serverFiltering && hasSource()) {

            clearTimeout(timer);
            timer = setTimeout(function () {
                loadOptions(input.value);
            }, 250);
        }
        else {
            renderOptions(input.value);
        }
    });

    input.addEventListener('keydown', function (ev) {

        const buttons = Array.from(
            optionsHost.querySelectorAll('.tech-multiselect-option'));

        if (ev.key === 'ArrowDown' || ev.key === 'ArrowUp') {

            ev.preventDefault();
            openPanel();

            if (buttons.length === 0) return;

            if (ev.key === 'ArrowDown') {
                activeIndex = (activeIndex + 1) % buttons.length;
            }
            else {
                activeIndex = activeIndex <= 0
                    ? buttons.length - 1
                    : activeIndex - 1;
            }

            buttons.forEach(function (button, index) {
                button.classList.toggle('is-active', index === activeIndex);
            });

            buttons[activeIndex].scrollIntoView({ block: 'nearest' });
        }
        else if (ev.key === 'Enter') {

            if (activeIndex >= 0 && buttons[activeIndex]) {

                ev.preventDefault();
                buttons[activeIndex].click();
            }
            else if (allowCustom && input.value.trim()) {

                ev.preventDefault();
                addCustom(input.value);
            }
        }
        else if (ev.key === 'Escape') {

            if (isOpen) {

                ev.preventDefault();
                ev.stopPropagation();
                closePanel();
            }
        }
        else if (ev.key === 'Backspace' &&
            !input.value && selected.length > 0) {

            removeValue(selected[selected.length - 1].value);
        }
        else if (ev.key === 'Tab') {
            closePanel();
        }
    });

    root._techMultiSelect = {
        getValues: function () {
            return selected.map(function (item) { return item.value; });
        },
        getItems: function () {
            return selected.map(function (item) {
                return { value: item.value, text: item.text };
            });
        },
        setValues: function (values) {

            selected = (values || []).map(function (value) {

                const text = String(value);

                const match = options.find(function (item) {
                    return item.value === text;
                });

                return { value: text, text: match ? match.text : text };
            });

            sync();
            renderOptions(input.value);
            notify();
        },
        add: function (value, text) {

            if (selected.some(function (item) {
                return item.value === String(value);
            })) return;

            if (maxItems && selected.length >= maxItems) return;

            selected.push({
                value: String(value),
                text: text ? String(text) : String(value)
            });

            sync();
            renderOptions(input.value);
            notify();
        },
        remove: removeValue,
        clear: function () {

            selected = [];
            sync();
            renderOptions(input.value);
            notify();
        },
        open: openPanel,
        close: closePanel,
        reload: function () {

            loaded = false;
            loadOptions(input.value);
        }
    };
}

Tech.register('multiselect', initMultiSelect);

// ============================================================
// FileUpload
// ============================================================

const techUploadUnits = ['B', 'KB', 'MB', 'GB'];

function techUploadFormatSize(bytes) {

    if (!bytes) return '0 B';

    let value = bytes;
    let index = 0;

    while (value >= 1024 && index < techUploadUnits.length - 1) {
        value /= 1024;
        index++;
    }

    const decimals = index === 0 || value >= 10 ? 0 : 1;

    return value.toFixed(decimals) + ' ' + techUploadUnits[index];
}

function techUploadIcon(file) {

    const extension = (file.name.split('.').pop() || '').toLowerCase();
    const type = (file.type || '').toLowerCase();

    if (type.indexOf('image/') === 0) return 'bi-file-earmark-image';
    if (extension === 'pdf') return 'bi-file-earmark-pdf';
    if (['zip', 'rar', '7z', 'gz', 'tar'].indexOf(extension) >= 0) {
        return 'bi-file-earmark-zip';
    }
    if (['doc', 'docx'].indexOf(extension) >= 0) return 'bi-file-earmark-word';
    if (['xls', 'xlsx', 'csv'].indexOf(extension) >= 0) {
        return 'bi-file-earmark-spreadsheet';
    }
    if (['ppt', 'pptx'].indexOf(extension) >= 0) return 'bi-file-earmark-slides';
    if (['txt', 'md', 'json', 'xml'].indexOf(extension) >= 0) {
        return 'bi-file-earmark-text';
    }

    return 'bi-file-earmark';
}

function techUploadMatches(file, accept) {

    if (!accept) return true;

    const name = (file.name || '').toLowerCase();
    const type = (file.type || '').toLowerCase();

    return accept.split(',').some(function (raw) {

        const token = raw.trim().toLowerCase();

        if (!token) return false;
        if (token === '*/*') return true;
        if (token.charAt(0) === '.') return name.endsWith(token);
        if (token.slice(-2) === '/*') return type.indexOf(token.slice(0, -1)) === 0;

        return type === token;
    });
}

function initFileUpload(root) {

    if (!root || root._techFileUpload) return;

    const input = root.querySelector('.tech-upload-input');
    const zone = root.querySelector('.tech-upload-zone');
    const list = root.querySelector('.tech-upload-list');
    const errorBox = root.querySelector('.tech-upload-error');
    const values = root.querySelector('.tech-upload-values');

    if (!input || !zone) return;

    const config = {
        name: root.dataset.name || input.name,
        url: root.dataset.url || '',
        field: root.dataset.field || 'file',
        accept: root.dataset.accept || input.accept || '',
        multiple: root.dataset.multiple === 'true',
        autoUpload: root.dataset.autoUpload !== 'false',
        thumbnails: root.dataset.thumbnails !== 'false',
        dragDrop: root.dataset.dragDrop !== 'false',
        required: root.dataset.required === 'true',
        emptyText: root.dataset.textRequired || 'انتخاب فایل الزامی است.',
        maxSize: Number(root.dataset.maxSize || 0) || 0,
        maxFiles: Number(root.dataset.maxFiles || 0) || 0,
        disabled: input.disabled || root.dataset.disabled === 'true'
    };

    const files = [];
    const queue = [];
    const objectUrls = new Map();

    let active = 0;
    let sequence = 0;
    let localOnly = false;

    if (config.url) {
        // Values are posted from the hidden list after a successful upload,
        // so the native control must not post the raw file on form submit.
        input.removeAttribute('name');
    }
    else {
        localOnly = true;
        config.autoUpload = false;
    }

    if (input.hasAttribute('required')) {
        // The script validates on submit so the message matches the rest of
        // the library; keep the native attribute as a no-JS fallback only.
        config.required = true;
        input.removeAttribute('required');
    }

    function publicFile(entry) {

        return {
            id: entry.id,
            file: entry.file,
            name: entry.name,
            size: entry.size,
            type: entry.type,
            status: entry.status,
            progress: Math.round(entry.progress),
            value: entry.value || '',
            url: entry.remoteUrl || '',
            message: entry.message || ''
        };
    }

    function visibleFiles() {

        return files.filter(function (entry) {
            return entry.status !== 'removed';
        });
    }

    function emit(name, payload) {

        Tech.trigger(root, name, payload);
    }

    function showError(message) {

        if (!errorBox) return;

        errorBox.textContent = message || '';
        errorBox.hidden = !message;

        root.classList.toggle('tech-upload-invalid', !!message);
    }

    function clearError() {

        showError('');
    }

    function statusText(entry) {

        switch (entry.status) {
            case 'uploading':
                return 'در حال آپلود…';
            case 'done':
                return 'آپلود شد';
            case 'ready':
                return 'انتخاب شد';
            case 'error':
                return entry.message || 'آپلود انجام نشد.';
            default:
                return 'آماده آپلود';
        }
    }

    function updateEntry(entry) {

        const nodes = entry.nodes;

        if (!nodes) return;

        const uploaded = entry.status === 'done';
        const failed = entry.status === 'error';
        const uploading = entry.status === 'uploading';
        const local = entry.status === 'ready';

        nodes.item.classList.toggle('is-uploading', uploading);
        nodes.item.classList.toggle('is-done', uploaded || local);
        nodes.item.classList.toggle('is-error', failed);

        nodes.status.textContent = statusText(entry);
        nodes.status.title = entry.status === 'error' ? entry.message || '' : '';

        if (uploading) {
            nodes.progress.hidden = false;
            nodes.bar.style.width = Math.min(100, entry.progress) + '%';
        }
        else if (uploaded && entry.progress > 0) {
            nodes.progress.hidden = false;
            nodes.bar.style.width = '100%';
        }
        else {
            nodes.progress.hidden = true;
            nodes.bar.style.width = '0%';
        }

        if (uploaded || local) {
            nodes.meta.textContent = techUploadFormatSize(entry.size);
            nodes.meta.hidden = false;
        }

        nodes.retry.hidden = !failed || !config.url;
        nodes.remove.hidden = false;
    }

    function buildEntryElement(entry) {

        const item = document.createElement('div');

        item.className = 'tech-upload-item';
        item.setAttribute('role', 'listitem');
        item.dataset.key = String(entry.id);

        const thumb = document.createElement('span');

        thumb.className = 'tech-upload-thumb';

        const isImage = (entry.type || '').indexOf('image/') === 0;

        if (config.thumbnails && isImage) {

            const image = document.createElement('img');

            const objectUrl = URL.createObjectURL(entry.file);

            objectUrls.set(entry.id, objectUrl);
            image.src = objectUrl;
            image.alt = entry.name;

            thumb.appendChild(image);
        }
        else {

            const icon = document.createElement('i');

            icon.className = 'bi ' + techUploadIcon(entry.file);
            thumb.appendChild(icon);
        }

        const info = document.createElement('div');

        info.className = 'tech-upload-info';

        const header = document.createElement('div');

        header.className = 'tech-upload-line';

        const name = document.createElement('span');

        name.className = 'tech-upload-name';
        name.textContent = entry.name;
        name.title = entry.name;

        const meta = document.createElement('span');

        meta.className = 'tech-upload-meta';
        meta.textContent = techUploadFormatSize(entry.size);

        header.appendChild(name);
        header.appendChild(meta);

        const status = document.createElement('div');

        status.className = 'tech-upload-status';

        const progress = document.createElement('div');

        progress.className = 'tech-upload-progress';
        progress.hidden = true;

        const bar = document.createElement('div');

        bar.className = 'tech-upload-bar';

        progress.appendChild(bar);

        info.appendChild(header);
        info.appendChild(status);
        info.appendChild(progress);

        const actions = document.createElement('div');

        actions.className = 'tech-upload-actions';

        const retry = document.createElement('button');

        retry.type = 'button';
        retry.className = 'tech-upload-retry';
        retry.title = 'آپلود دوباره';
        retry.setAttribute('aria-label', 'آپلود دوباره');
        retry.hidden = true;

        const retryIcon = document.createElement('i');

        retryIcon.className = 'bi bi-arrow-clockwise';
        retry.appendChild(retryIcon);

        const remove = document.createElement('button');

        remove.type = 'button';
        remove.className = 'tech-upload-remove';
        remove.title = 'حذف';
        remove.setAttribute('aria-label', 'حذف فایل');

        const removeIcon = document.createElement('i');

        removeIcon.className = 'bi bi-x-lg';
        remove.appendChild(removeIcon);

        actions.appendChild(retry);
        actions.appendChild(remove);

        item.appendChild(thumb);
        item.appendChild(info);
        item.appendChild(actions);

        entry.nodes = {
            item: item,
            meta: meta,
            status: status,
            progress: progress,
            bar: bar,
            retry: retry,
            remove: remove
        };

        retry.addEventListener('click', function (event) {

            event.preventDefault();
            event.stopPropagation();

            if (entry.status !== 'error') return;

            entry.status = 'pending';
            entry.message = '';
            entry.progress = 0;

            updateEntry(entry);
            enqueue(entry);
        });

        remove.addEventListener('click', function (event) {

            event.preventDefault();
            event.stopPropagation();

            removeEntry(entry);
        });

        return item;
    }

    function appendEntry(entry) {

        files.push(entry);

        if (list) {

            const element = buildEntryElement(entry);

            entry.element = element;
            list.appendChild(element);
        }

        updateEntry(entry);
    }

    function destroyEntry(entry) {

        if (entry.xhr) {

            try {
                entry.xhr.abort();
            }
            catch {
                /* already finished */
            }

            entry.xhr = null;
        }

        const objectUrl = objectUrls.get(entry.id);

        if (objectUrl) {

            URL.revokeObjectURL(objectUrl);
            objectUrls.delete(entry.id);
        }

        if (entry.element && entry.element.parentNode) {
            entry.element.parentNode.removeChild(entry.element);
        }

        if (entry.hidden) {

            entry.hidden.remove();
            entry.hidden = null;
        }

        entry.status = 'removed';
    }

    function removeEntry(entry) {

        destroyEntry(entry);

        const index = files.indexOf(entry);

        if (index >= 0) files.splice(index, 1);

        syncNativeInput();
        clearError();

        emit('Remove', publicFile(entry));
        emit('Change', { files: visibleFiles().map(publicFile) });
        updateStartButton();
    }

    function clearEntryMessages() {

        clearError();
    }

    function acceptsMore(valid) {

        if (!config.multiple && valid.length > 0) {
            valid.length = 1;
        }

        if (config.maxFiles > 0) {

            const activeCount = visibleFiles().length;
            const room = config.maxFiles - activeCount;

            if (room <= 0) return [];
            if (valid.length > room) valid.length = room;
        }

        return valid;
    }

    function addFiles(fileList) {

        if (!fileList || !fileList.length || config.disabled) return;

        clearEntryMessages();

        const incoming = Array.from(fileList);
        const accepted = [];
        const failures = [];

        incoming.forEach(function (file) {

            if (!techUploadMatches(file, config.accept)) {
                failures.push(file.name + ': فرمت فایل مجاز نیست.');
                return;
            }

            if (config.maxSize > 0 && file.size > config.maxSize) {
                failures.push(file.name + ': حجم فایل بیشتر از '
                    + techUploadFormatSize(config.maxSize) + ' است.');
                return;
            }

            const duplicate = visibleFiles().some(function (entry) {
                return entry.name === file.name
                    && entry.size === file.size
                    && entry.lastModified === file.lastModified;
            }) || accepted.some(function (candidate) {
                return candidate.name === file.name
                    && candidate.size === file.size
                    && candidate.lastModified === file.lastModified;
            });

            if (duplicate) {
                failures.push(file.name + ': این فایل قبلاً انتخاب شده است.');
                return;
            }

            accepted.push(file);
        });

        const room = config.maxFiles > 0
            ? config.maxFiles - visibleFiles().length
            : -1;

        if (room === 0 && accepted.length > 0) {
            failures.push('حداکثر ' + config.maxFiles + ' فایل مجاز است.');
            accepted.length = 0;
        }
        else if (room > 0 && accepted.length > room) {
            failures.push('حداکثر ' + config.maxFiles + ' فایل مجاز است.');
            accepted.length = room;
        }

        const selected = acceptsMore(accepted);

        if (selected.length > 0 && !config.multiple) {
            visibleFiles().forEach(destroyEntry);
            files.length = 0;
        }

        selected.forEach(function (file) {

            const entry = {
                id: ++sequence,
                file: file,
                name: file.name,
                size: file.size,
                type: file.type || '',
                lastModified: file.lastModified || 0,
                status: 'pending',
                progress: 0,
                message: '',
                value: '',
                remoteUrl: '',
                xhr: null,
                element: null,
                nodes: null,
                hidden: null
            };

            appendEntry(entry);
        });

        if (failures.length > 0) {
            showError(failures.join(' '));
        }

        const added = files.length > 0
            ? files.filter(function (entry) {
                return entry.status !== 'removed'
                    && selected.some(function (file) {
                        return file === entry.file;
                    });
            })
            : [];

        if (added.length === 0) {
            input.value = '';
            return;
        }

        emit('Change', { files: visibleFiles().map(publicFile) });

        if (localOnly) {

            added.forEach(function (entry) {

                entry.status = 'ready';
                updateEntry(entry);
            });

            syncNativeInput();
        }
        else {

            input.value = '';

            if (config.autoUpload) {
                added.forEach(enqueue);
            }
        }

        updateStartButton();
    }

    function syncNativeInput() {

        if (!localOnly || !input) return;

        try {

            const transfer = new DataTransfer();

            visibleFiles().forEach(function (entry) {
                transfer.items.add(entry.file);
            });

            input.files = transfer.files;
        }
        catch {
            // Browsers without DataTransfer file support keep the native
            // picker as the only way to change the selection.
        }
    }

    function addHiddenValue(entry, value) {

        if (!values || !value) return;

        const hidden = document.createElement('input');

        hidden.type = 'hidden';
        hidden.name = config.name;
        hidden.value = String(value);
        hidden.className = 'tech-upload-value';
        hidden.dataset.key = String(entry.id);

        values.appendChild(hidden);
        entry.hidden = hidden;
    }

    function uploadEntry(entry) {

        if (!config.url || entry.status === 'removed') return;

        entry.status = 'uploading';
        entry.progress = 0;
        entry.message = '';

        updateEntry(entry);
        emit('Upload', publicFile(entry));

        const data = new FormData();

        data.append(config.field, entry.file, entry.name);

        const token = Tech.getAntiforgeryToken(root);
        const xhr = new XMLHttpRequest();

        entry.xhr = xhr;
        active++;

        xhr.open('POST', config.url, true);
        xhr.setRequestHeader('Accept', 'application/json');

        if (token) {

            data.append('__RequestVerificationToken', token);
            xhr.setRequestHeader('RequestVerificationToken', token);
        }

        xhr.upload.addEventListener('progress', function (event) {

            if (!event.lengthComputable) return;

            entry.progress = (event.loaded / event.total) * 100;
            updateEntry(entry);
        });

        xhr.addEventListener('load', function () {

            active--;
            entry.xhr = null;

            if (entry.status === 'removed') {
                pump();
                return;
            }

            let result = null;

            try {
                result = xhr.responseText
                    ? JSON.parse(xhr.responseText)
                    : null;
            }
            catch {
                result = xhr.responseText;
            }

            if (xhr.status >= 200 && xhr.status < 300) {

                entry.status = 'done';
                entry.progress = 100;
                entry.value = resolveUploadValue(result, entry);
                entry.remoteUrl = result && typeof result === 'object'
                    ? String(result.url || '')
                    : '';

                addHiddenValue(entry, entry.value);
                updateEntry(entry);

                emit('Success', {
                    file: publicFile(entry),
                    response: result
                });
            }
            else {

                entry.status = 'error';
                entry.message = resolveUploadError(result, xhr);
                entry.progress = 0;

                updateEntry(entry);
                showError(entry.message);

                emit('Error', {
                    file: publicFile(entry),
                    message: entry.message,
                    response: result
                });
            }

            pump();
        });

        xhr.addEventListener('error', function () {

            active--;
            entry.xhr = null;

            if (entry.status === 'removed') {
                pump();
                return;
            }

            entry.status = 'error';
            entry.message = 'ارتباط با سرور برقرار نشد.';
            entry.progress = 0;

            updateEntry(entry);
            showError(entry.message);

            emit('Error', {
                file: publicFile(entry),
                message: entry.message,
                response: null
            });

            pump();
        });

        xhr.addEventListener('abort', function () {

            if (entry.xhr === xhr) {
                active = Math.max(0, active - 1);
                entry.xhr = null;
                pump();
            }
        });

        try {
            xhr.send(data);
        }
        catch {

            active--;
            entry.xhr = null;
            entry.status = 'error';
            entry.message = 'ارسال فایل انجام نشد.';

            updateEntry(entry);
            showError(entry.message);

            emit('Error', {
                file: publicFile(entry),
                message: entry.message,
                response: null
            });

            pump();
        }
    }

    function resolveUploadValue(result, entry) {

        if (result && typeof result === 'object') {

            const value = result.id ?? result.value ?? result.name;

            if (value !== undefined && value !== null && value !== '') {
                return String(value);
            }
        }

        if (typeof result === 'string' && result.trim() !== '') {
            return result.trim();
        }

        return entry.name;
    }

    function resolveUploadError(result, xhr) {

        if (result && typeof result === 'object') {

            const message = result.message || result.error || result.title;

            if (message) return String(message);
        }

        if (typeof result === 'string' && result.trim() !== ''
            && result.trim().charAt(0) !== '{'
            && result.trim().charAt(0) !== '<') {

            return result.trim().slice(0, 300);
        }

        if (xhr.status === 413) {
            return 'حجم فایل برای سرور قابل قبول نیست.';
        }

        return 'آپلود انجام نشد. (HTTP ' + xhr.status + ')';
    }

    function enqueue(entry) {

        if (entry.status === 'removed') return;

        if (localOnly) {

            entry.status = 'ready';
            updateEntry(entry);
            return;
        }

        entry.status = 'pending';
        queue.push(entry);
        updateEntry(entry);
        pump();
    }

    function pump() {

        if (active > 0) return;

        let next = null;

        while (queue.length > 0) {

            const candidate = queue.shift();

            if (candidate.status === 'pending') {
                next = candidate;
                break;
            }
        }

        if (!next) {

            if (!localOnly) {
                emit('Complete', { files: visibleFiles().map(publicFile) });
            }

            updateStartButton();
            return;
        }

        uploadEntry(next);
    }

    let startButton = null;

    function updateStartButton() {

        if (!startButton) return;

        const pending = visibleFiles().some(function (entry) {
            return entry.status === 'pending' || entry.status === 'error';
        });

        startButton.hidden = !pending;
    }

    if (config.url && !config.autoUpload && list) {

        startButton = document.createElement('button');

        startButton.type = 'button';
        startButton.className = 'tech-upload-start';
        startButton.hidden = true;
        startButton.textContent = 'شروع آپلود';

        startButton.addEventListener('click', function () {

            const pending = visibleFiles().filter(function (entry) {
                return entry.status === 'pending' || entry.status === 'error';
            });

            pending.forEach(function (entry) {

                entry.status = 'pending';
                entry.message = '';
                entry.progress = 0;

                updateEntry(entry);
                queue.push(entry);
            });

            pump();
        });

        list.parentNode.insertBefore(startButton, list.nextSibling);
    }

    function validateRequired() {

        if (config.disabled) return true;

        const waiting = visibleFiles().some(function (entry) {
            return entry.status === 'uploading' || entry.status === 'pending';
        });

        if (waiting) {

            showError('آپلود فایل‌ها هنوز کامل نشده است.');
            return false;
        }

        if (!config.required) return true;

        const completed = visibleFiles().some(function (entry) {
            return entry.status === 'done' || entry.status === 'ready';
        });

        if (completed) return true;

        showError(config.emptyText);
        return false;
    }

    const form = root.closest('form');

    if (form) {

        form.addEventListener('submit', function (event) {

            if (!validateRequired()) {
                event.preventDefault();
                event.stopPropagation();
            }
        }, true);
    }

    input.addEventListener('change', function () {

        addFiles(input.files);

        if (!localOnly) input.value = '';
    });

    if (config.dragDrop) {

        ['dragenter', 'dragover'].forEach(function (name) {

            zone.addEventListener(name, function (event) {

                if (config.disabled) return;

                event.preventDefault();

                if (event.dataTransfer) {
                    event.dataTransfer.dropEffect = 'copy';
                }

                root.classList.add('tech-upload-dragover');
            });
        });

        zone.addEventListener('dragleave', function (event) {

            if (event.relatedTarget && zone.contains(event.relatedTarget)) {
                return;
            }

            root.classList.remove('tech-upload-dragover');
        });

        zone.addEventListener('drop', function (event) {

            if (config.disabled) return;

            event.preventDefault();
            root.classList.remove('tech-upload-dragover');

            if (event.dataTransfer && event.dataTransfer.files) {
                addFiles(event.dataTransfer.files);
            }
        });
    }

    root._techFileUpload = {
        element: root,
        addFiles: function (fileList) {
            return addFiles(fileList);
        },
        upload: function () {

            const pending = visibleFiles().filter(function (entry) {
                return entry.status === 'pending' || entry.status === 'error';
            });

            pending.forEach(function (entry) {

                entry.status = 'pending';
                entry.message = '';
                entry.progress = 0;

                updateEntry(entry);
                queue.push(entry);
            });

            pump();
        },
        clear: function () {

            queue.length = 0;

            visibleFiles().forEach(destroyEntry);

            files.length = 0;

            syncNativeInput();
            clearError();
            updateStartButton();

            emit('Change', { files: [] });
        },
        remove: function (id) {

            const entry = files.find(function (item) {
                return String(item.id) === String(id);
            });

            if (entry) removeEntry(entry);
        },
        getFiles: function () {
            return visibleFiles().map(publicFile);
        }
    };

    clearError();
}

Tech.FileUpload = {
    instances: new Map(),
    get: function (element) {

        if (!element) return null;

        if (typeof element === 'string') {
            element = document.getElementById(element);
        }

        return element ? element._techFileUpload || null : null;
    }
};

Tech.register('fileupload', function (root) {

    initFileUpload(root);

    if (root._techFileUpload) {
        Tech.FileUpload.instances.set(root.id || root, root._techFileUpload);
    }
});

// ============================================================
// Captcha
// ============================================================

Tech.Captcha = Tech.Captcha || {};
Tech.Captcha.instances = Tech.Captcha.instances || new Map();

Tech.Captcha.get = function (id) {

    return Tech.Captcha.instances.get(id) || null;
};

Tech.Captcha.refresh = function (id) {

    const instance = Tech.Captcha.get(id);

    return instance ? instance.refresh() : null;
};

Tech.Captcha.token = function (id) {

    const instance = Tech.Captcha.get(id);

    return instance ? instance.token() : null;
};

function techCaptchaParams(root) {

    const params = new URLSearchParams();

    params.set('language', root.dataset.language || 'fa');
    params.set('kind', root.dataset.kind || 'letters');

    if (root.dataset.noise) {
        params.set('noise', root.dataset.noise);
    }

    if (root.dataset.kind === 'math') {
        params.set('operation', root.dataset.operation || 'addition');
    }
    else if (root.dataset.length) {
        params.set('length', root.dataset.length);
    }

    return params;
}

function techCaptchaClearError(root, input) {

    if (input) {
        input.classList.remove('is-invalid');
        input.removeAttribute('aria-invalid');
    }

    const error = root.querySelector('.tech-captcha-error');

    if (error) error.hidden = true;
}

function techCaptchaRefresh(root) {

    const input = root.querySelector('.tech-captcha-answer');
    const hidden = root.querySelector('input[type="hidden"]');
    const image = root.querySelector('.tech-captcha-image');
    const button = root.querySelector('.tech-captcha-refresh');
    const url = root.dataset.refreshUrl || '/techui/captcha';
    const separator = url.indexOf('?') >= 0 ? '&' : '?';

    if (!hidden || !image) return Promise.resolve(null);

    if (button) button.disabled = true;

    root.classList.add('is-loading');

    return fetch(url + separator + techCaptchaParams(root).toString(), {
        headers: { 'Accept': 'application/json' },
        credentials: 'same-origin'
    })
        .then(function (response) {

            if (!response.ok) {
                throw new Error('HTTP ' + response.status);
            }

            return response.json();
        })
        .then(function (data) {

            if (!data || !data.token || !data.image) return null;

            hidden.value = data.token;
            image.src = data.image;

            if (input) {
                input.value = '';
                techCaptchaClearError(root, input);
            }

            return data;
        })
        .catch(function (error) {

            Tech.trigger(root, 'Error', { error: error });

            return null;
        })
        .finally(function () {

            if (button) button.disabled = false;

            root.classList.remove('is-loading');
        });
}

function initCaptcha(root) {

    if (root._techCaptchaReady) return;

    root._techCaptchaReady = true;

    const input = root.querySelector('.tech-captcha-answer');
    const hidden = root.querySelector('input[type="hidden"]');
    const button = root.querySelector('.tech-captcha-refresh');

    if (!input || !hidden) return;

    if (button) {
        button.addEventListener('click', function () {

            techCaptchaRefresh(root).then(function () {
                input.focus();
            });
        });
    }

    input.addEventListener('input', function () {
        techCaptchaClearError(root, input);
    });

    Tech.Captcha.instances.set(input.id || root.id, {
        element: root,
        answer: function () { return input.value; },
        token: function () { return hidden.value; },
        refresh: function () { return techCaptchaRefresh(root); }
    });
}

Tech.register('captcha', initCaptcha);

// ============================================================
// Toast / notifications
// ============================================================

const techToastIcons = {
    success: 'check-circle-fill',
    error: 'x-circle-fill',
    warning: 'exclamation-triangle-fill',
    info: 'info-circle-fill'
};

const techToastPositions = {
    'top-start': 'top-start',
    'top-center': 'top-center',
    'top-end': 'top-end',
    'bottom-start': 'bottom-start',
    'bottom-center': 'bottom-center',
    'bottom-end': 'bottom-end'
};

function techToastContainer(position) {

    const key = techToastPositions[position] || 'top-end';
    const selector = '.tech-toast-container[data-position="' + key + '"]';

    let container = document.querySelector(selector);

    if (!container) {

        container = document.createElement('div');

        container.className = 'tech-toast-container tech-toast-' + key;
        container.dataset.position = key;
        container.setAttribute('aria-live', 'polite');
        container.setAttribute('aria-atomic', 'false');

        document.body.appendChild(container);
    }

    return container;
}

Tech.notify = function (message, options) {

    options = options || {};

    const type = techToastIcons[options.type] ? options.type : 'info';
    const duration = options.duration === undefined
        ? 4200
        : Math.max(0, Number(options.duration) || 0);
    const container = techToastContainer(options.position);

    const toast = document.createElement('div');

    toast.className = 'tech-toast tech-toast-' + type;
    toast.setAttribute('role', 'status');

    const icon = document.createElement('i');

    icon.className = 'bi bi-' + techToastIcons[type] + ' tech-toast-icon';

    const body = document.createElement('div');

    body.className = 'tech-toast-body';

    if (options.title) {

        const title = document.createElement('div');

        title.className = 'tech-toast-title';
        title.textContent = options.title;

        body.appendChild(title);
    }

    const text = document.createElement('div');

    text.className = 'tech-toast-message';
    text.textContent = message === undefined || message === null
        ? ''
        : String(message);

    body.appendChild(text);

    toast.appendChild(icon);
    toast.appendChild(body);

    if (options.dismissible !== false) {

        const close = document.createElement('button');

        close.type = 'button';
        close.className = 'tech-toast-close';
        close.setAttribute('aria-label', 'بستن');
        close.innerHTML = '<i class="bi bi-x-lg"></i>';

        toast.appendChild(close);
    }

    let progress = null;

    if (options.progress !== false && duration > 0) {

        progress = document.createElement('span');
        progress.className = 'tech-toast-progress';
        progress.style.animationDuration = duration + 'ms';

        toast.appendChild(progress);
    }

    container.appendChild(toast);

    let timer = null;
    let remaining = duration;
    let started = 0;
    let closed = false;

    function dismiss() {

        if (closed) return;

        closed = true;

        clearTimeout(timer);

        toast.classList.add('is-leaving');

        setTimeout(function () {
            toast.remove();
        }, 200);
    }

    function startTimer() {

        if (duration <= 0) return;

        started = Date.now();

        timer = setTimeout(dismiss, remaining);
    }

    function pauseTimer() {

        if (duration <= 0 || closed) return;

        clearTimeout(timer);
        remaining -= Date.now() - started;

        if (remaining < 0) remaining = 0;

        if (progress) progress.style.animationPlayState = 'paused';
    }

    function resumeTimer() {

        if (duration <= 0 || closed) return;

        if (progress) progress.style.animationPlayState = 'running';

        startTimer();
    }

    const closeButton = toast.querySelector('.tech-toast-close');

    if (closeButton) closeButton.addEventListener('click', dismiss);

    toast.addEventListener('mouseenter', pauseTimer);
    toast.addEventListener('mouseleave', resumeTimer);

    startTimer();

    return {
        element: toast,
        close: dismiss
    };
};

Tech.toast = Tech.notify;

// ============================================================
// Dialog / confirm / alert
// ============================================================

const techDialogIcons = {
    info: 'info-circle',
    success: 'check-circle',
    warning: 'exclamation-triangle',
    danger: 'x-octagon',
    question: 'question-circle'
};

Tech.dialog = function (options) {

    options = options || {};

    return new Promise(function (resolve) {

        const type = techDialogIcons[options.type] ? options.type : 'info';
        const closable = options.closable !== false;

        const backdrop = document.createElement('div');

        backdrop.className = 'tech-dialog-backdrop';

        const dialog = document.createElement('div');

        dialog.className = 'tech-dialog tech-dialog-' + type;

        if (options.size === 'sm' || options.size === 'lg') {
            dialog.classList.add('tech-dialog-' + options.size);
        }

        dialog.setAttribute('role', 'dialog');
        dialog.setAttribute('aria-modal', 'true');

        const header = document.createElement('div');

        header.className = 'tech-dialog-header';

        const icon = document.createElement('i');

        icon.className = 'bi bi-' + techDialogIcons[type] +
            ' tech-dialog-icon';

        const title = document.createElement('div');

        title.className = 'tech-dialog-title';
        title.textContent = options.title || '';

        header.appendChild(icon);
        header.appendChild(title);

        let closeButton = null;

        if (closable) {

            closeButton = document.createElement('button');

            closeButton.type = 'button';
            closeButton.className = 'tech-dialog-close';
            closeButton.setAttribute('aria-label', 'بستن');
            closeButton.innerHTML = '<i class="bi bi-x-lg"></i>';

            header.appendChild(closeButton);
        }

        const body = document.createElement('div');

        body.className = 'tech-dialog-body';

        if (options.content !== undefined && options.content !== null) {
            body.innerHTML = String(options.content);
        }
        else {

            const text = document.createElement('p');

            text.className = 'tech-dialog-message';
            text.textContent = options.message === undefined
                ? ''
                : String(options.message);

            body.appendChild(text);
        }

        const footer = document.createElement('div');

        footer.className = 'tech-dialog-footer';

        const buttons = Array.isArray(options.buttons) && options.buttons.length
            ? options.buttons
            : [
                { text: 'انصراف', value: false, variant: 'outline-secondary' },
                { text: 'تأیید', value: true, variant: 'primary' }
            ];

        buttons.forEach(function (config) {

            const button = document.createElement('button');

            button.type = 'button';
            button.className = 'btn btn-' + (config.variant || 'primary');

            if (config.className) {
                button.className += ' ' + config.className;
            }

            button.textContent = config.text || 'باشه';

            button.addEventListener('click', function () {

                if (typeof config.action === 'function') {
                    config.action(close);
                    return;
                }

                close(config.value);
            });

            footer.appendChild(button);
        });

        dialog.appendChild(header);
        dialog.appendChild(body);

        if (footer.children.length > 0) {
            dialog.appendChild(footer);
        }

        backdrop.appendChild(dialog);

        let finished = false;

        function onKeyDown(ev) {

            if (ev.key === 'Escape' && closable) {

                ev.preventDefault();
                ev.stopPropagation();
                close(null);
            }
        }

        function close(value) {

            if (finished) return;

            finished = true;

            document.removeEventListener('keydown', onKeyDown, true);

            backdrop.classList.add('is-leaving');

            setTimeout(function () {
                backdrop.remove();
            }, 160);

            resolve(value);
        }

        if (closeButton) {
            closeButton.addEventListener('click', function () {
                close(null);
            });
        }

        backdrop.addEventListener('mousedown', function (ev) {

            if (closable && ev.target === backdrop) close(null);
        });

        document.addEventListener('keydown', onKeyDown, true);
        document.body.appendChild(backdrop);

        const focusTarget = footer.querySelector('.btn-primary') ||
            footer.querySelector('button') ||
            closeButton;

        if (focusTarget) focusTarget.focus();

        if (typeof options.onOpen === 'function') {
            options.onOpen({ dialog: dialog, close: close });
        }
    });
};

Tech.confirm = function (message, options) {

    options = options || {};

    return Tech.dialog({
        title: options.title || 'تأیید',
        message: message,
        type: options.type || 'question',
        size: options.size,
        closable: options.closable,
        buttons: options.buttons || [
            { text: options.cancelText || 'انصراف', value: false, variant: 'outline-secondary' },
            { text: options.confirmText || 'تأیید', value: true, variant: options.confirmVariant || 'primary' }
        ]
    }).then(function (value) {
        return value === true;
    });
};

Tech.alert = function (message, options) {

    options = options || {};

    return Tech.dialog({
        title: options.title || 'توجه',
        message: message,
        type: options.type || 'info',
        size: options.size,
        buttons: [
            {
                text: options.buttonText || 'باشه',
                value: true,
                variant: 'primary'
            }
        ]
    });
};

// ============================================================
// Chart
// ============================================================

Tech.Chart = Tech.Chart || {};
Tech.Chart.instances = Tech.Chart.instances || new Map();

(function () {

    const NS = 'http://www.w3.org/2000/svg';

    const DEFAULT_PALETTE = [
        '#2563eb', '#059669', '#d97706', '#7c3aed',
        '#e11d48', '#0891b2', '#65a30d', '#ea580c'
    ];

    const SPARE_COLORS = [
        '#0891b2', '#db2777', '#65a30d', '#ea580c',
        '#0d9488', '#4f46e5', '#a16207', '#be123c'
    ];

    const LINE_TYPES = {
        line: true,
        spline: true,
        step: true,
        area: true,
        areaspline: true
    };

    const BAR_TYPES = { column: true, bar: true };

    const CARTESIAN_TYPES = {
        line: true, spline: true, step: true, area: true,
        areaspline: true, column: true, bar: true,
        waterfall: true, candlestick: true
    };

    function create(tag, attrs, parent) {

        const node = document.createElementNS(NS, tag);

        setAttrs(node, attrs);

        if (parent) parent.appendChild(node);

        return node;
    }

    function setAttrs(node, attrs) {

        if (!attrs) return node;

        for (const key in attrs) {

            const value = attrs[key];

            if (value === null || value === undefined) continue;

            node.setAttribute(key, value);
        }

        return node;
    }

    function text(parent, value, attrs) {

        const node = create('text', attrs, parent);

        node.textContent = value === null || value === undefined
            ? ''
            : String(value);

        return node;
    }

    // SVG text-anchor semantics flip with direction: in RTL "end" is the
    // left edge, so anchors must be chosen by the side the text grows to.
    function textAnchor(dir, extend) {

        if (extend === 'right') {
            return dir ? 'end' : 'start';
        }

        return dir ? 'start' : 'end';
    }

    function round(value) {
        return Math.round(value * 100) / 100;
    }

    function num(value) {

        if (value === null || value === undefined || value === '') {
            return null;
        }

        const parsed = Number(value);

        return Number.isFinite(parsed) ? parsed : null;
    }

    function clamp(value, min, max) {
        return Math.min(max, Math.max(min, value));
    }

    function estimate(value, fontSize) {
        return String(value === null || value === undefined ? '' : value)
            .length * fontSize * 0.58;
    }

    function maxOf(values) {

        let max = -Infinity;

        values.forEach(function (value) {
            if (value !== null && Number.isFinite(value)) {
                max = Math.max(max, value);
            }
        });

        return max;
    }

    function sumOf(values) {

        let sum = 0;

        values.forEach(function (value) {
            if (value !== null && Number.isFinite(value)) {
                sum += value;
            }
        });

        return sum;
    }

    function cssVar(element, name, fallback) {

        const value = getComputedStyle(element)
            .getPropertyValue(name)
            .trim();

        return value || fallback;
    }

    function resolvePalette(element, custom) {

        if (Array.isArray(custom) && custom.length > 0) {
            return custom.slice();
        }

        const style = getComputedStyle(element);
        const palette = [];

        for (let index = 1; index <= 8; index++) {

            const value = style
                .getPropertyValue('--tech-chart-' + index)
                .trim();

            palette.push(value || DEFAULT_PALETTE[index - 1]);
        }

        // Preset primaries can collide with palette slots (e.g. emerald
        // equals --tech-chart-2); keep series visually distinguishable.
        const available = new Set(palette.map(function (color) {
            return color.toLowerCase();
        }));

        const spares = SPARE_COLORS.filter(function (color) {
            return !available.has(color.toLowerCase());
        });

        const used = new Set();
        let spareIndex = 0;

        return palette.map(function (color) {

            const key = color.toLowerCase();

            if (!used.has(key)) {
                used.add(key);
                return color;
            }

            let replacement = null;

            while (spareIndex < spares.length) {

                const candidate = spares[spareIndex++];

                if (!used.has(candidate.toLowerCase())) {
                    replacement = candidate;
                    break;
                }
            }

            replacement = replacement || color;

            used.add(replacement.toLowerCase());

            return replacement;
        });
    }

    function resolveLocale(root, options) {

        if (options.locale) return options.locale;

        const holder = root.closest('[lang]');
        const lang = holder ? holder.getAttribute('lang') : null;

        if (lang) return lang;

        return document.documentElement.lang || 'fa-IR';
    }

    function formatValue(value, format, locale) {

        if (value === null || value === undefined) return '';

        const number = Number(value);

        if (!Number.isFinite(number)) return '';

        const options = {
            maximumFractionDigits: Math.abs(number) >= 1000 ? 1 : 2
        };

        let formatted;

        try {

            if (format === 'compact') {

                formatted = new Intl.NumberFormat(locale, {
                    notation: 'compact',
                    maximumFractionDigits: 1
                }).format(number);
            }
            else {
                formatted = new Intl.NumberFormat(locale, options)
                    .format(number);
            }
        }
        catch {
            formatted = String(number);
        }

        if (format === 'percent') return formatted + '٪';
        if (format === 'currency') return formatted + ' ریال';

        return formatted;
    }

    function niceNumber(range, roundResult) {

        const exponent = Math.floor(Math.log10(Math.max(range, 1e-9)));
        const fraction = range / Math.pow(10, exponent);

        let nice;

        if (roundResult) {
            if (fraction < 1.5) nice = 1;
            else if (fraction < 3) nice = 2;
            else if (fraction < 7) nice = 5;
            else nice = 10;
        }
        else {
            if (fraction <= 1) nice = 1;
            else if (fraction <= 2) nice = 2;
            else if (fraction <= 5) nice = 5;
            else nice = 10;
        }

        return nice * Math.pow(10, exponent);
    }

    function niceScale(min, max, count) {

        if (!Number.isFinite(min)) min = 0;
        if (!Number.isFinite(max)) max = 1;
        if (min === max) max = min + 1;

        const range = niceNumber(max - min, false);
        const step = niceNumber(range / Math.max(2, count - 1), true);
        const niceMin = Math.floor(min / step) * step;
        const niceMax = Math.ceil(max / step) * step;

        const ticks = [];

        for (let value = niceMin;
            value <= niceMax + step * 0.5;
            value += step) {

            ticks.push(Number(value.toFixed(10)));
        }

        return {
            min: niceMin,
            max: niceMax === niceMin ? niceMin + step : niceMax,
            step: step,
            ticks: ticks
        };
    }

    function buildScale(minValue, maxValue, axis) {

        axis = axis || {};

        let min = axis.min !== null && axis.min !== undefined
            ? Number(axis.min)
            : minValue;

        let max = axis.max !== null && axis.max !== undefined
            ? Number(axis.max)
            : maxValue;

        if (!Number.isFinite(min)) min = 0;
        if (!Number.isFinite(max)) max = 1;

        if (axis.beginAtZero === true) {
            min = Math.min(0, min);
            max = Math.max(0, max);
        }
        else if (axis.beginAtZero === null ||
            axis.beginAtZero === undefined) {

            if (min >= 0) min = 0;
        }

        if (max <= min) max = min + 1;

        return niceScale(min, max, 5);
    }

    function polar(cx, cy, radius, degrees) {

        const radians = degrees * Math.PI / 180;

        return {
            x: cx + radius * Math.cos(radians),
            y: cy + radius * Math.sin(radians)
        };
    }

    function arcPath(cx, cy, radius, start, end) {

        const from = polar(cx, cy, radius, start);
        const to = polar(cx, cy, radius, end);
        const large = Math.abs(end - start) > 180 ? 1 : 0;
        const sweep = end >= start ? 1 : 0;

        return 'M' + round(from.x) + ' ' + round(from.y) +
            'A' + round(radius) + ' ' + round(radius) +
            ' 0 ' + large + ' ' + sweep + ' ' +
            round(to.x) + ' ' + round(to.y);
    }

    function ringSlice(cx, cy, inner, outer, start, end) {

        const o1 = polar(cx, cy, outer, start);
        const o2 = polar(cx, cy, outer, end);
        const i2 = polar(cx, cy, inner, end);
        const i1 = polar(cx, cy, inner, start);
        const large = Math.abs(end - start) > 180 ? 1 : 0;

        return 'M' + round(o1.x) + ' ' + round(o1.y) +
            'A' + round(outer) + ' ' + round(outer) +
            ' 0 ' + large + ' 1 ' + round(o2.x) + ' ' + round(o2.y) +
            'L' + round(i2.x) + ' ' + round(i2.y) +
            'A' + round(inner) + ' ' + round(inner) +
            ' 0 ' + large + ' 0 ' + round(i1.x) + ' ' + round(i1.y) +
            'Z';
    }

    function wedgeSlice(cx, cy, radius, start, end) {

        const from = polar(cx, cy, 0, start);
        const to = polar(cx, cy, radius, end);
        const ring = polar(cx, cy, radius, start);
        const large = Math.abs(end - start) > 180 ? 1 : 0;

        return 'M' + round(from.x) + ' ' + round(from.y) +
            'L' + round(ring.x) + ' ' + round(ring.y) +
            'A' + round(radius) + ' ' + round(radius) +
            ' 0 ' + large + ' 1 ' + round(to.x) + ' ' + round(to.y) +
            'Z';
    }

    function linePath(points, mode) {

        if (!points || points.length === 0) return '';

        if (points.length === 1) {
            return 'M' + round(points[0].x) + ' ' + round(points[0].y);
        }

        let path = 'M' + round(points[0].x) + ' ' + round(points[0].y);

        if (mode === 'step') {

            for (let index = 1; index < points.length; index++) {
                path += 'H' + round(points[index].x) +
                    'V' + round(points[index].y);
            }

            return path;
        }

        if (mode !== 'smooth') {

            for (let index = 1; index < points.length; index++) {
                path += 'L' + round(points[index].x) +
                    ' ' + round(points[index].y);
            }

            return path;
        }

        for (let index = 0; index < points.length - 1; index++) {

            const p0 = points[index - 1] || points[index];
            const p1 = points[index];
            const p2 = points[index + 1];
            const p3 = points[index + 2] || p2;

            const c1x = p1.x + (p2.x - p0.x) / 6;
            const c1y = p1.y + (p2.y - p0.y) / 6;
            const c2x = p2.x - (p3.x - p1.x) / 6;
            const c2y = p2.y - (p3.y - p1.y) / 6;

            path += 'C' + round(c1x) + ' ' + round(c1y) +
                ' ' + round(c2x) + ' ' + round(c2y) +
                ' ' + round(p2.x) + ' ' + round(p2.y);
        }

        return path;
    }

    function areaPath(points, mode, baseline) {

        if (!points || points.length === 0) return '';

        const line = linePath(points, mode);
        const first = points[0];
        const last = points[points.length - 1];

        return line +
            'L' + round(last.x) + ' ' + round(baseline) +
            'L' + round(first.x) + ' ' + round(baseline) + 'Z';
    }

    function toggleSet(set, key) {

        if (set.has(key)) set.delete(key);
        else set.add(key);
    }

    function removeLegend(root) {

        const legend = root.querySelector('.tech-chart-legend');

        if (legend) legend.remove();
    }

    function drawLegend(ctx, items, hiddenSet, toggle) {

        removeLegend(ctx.root);

        if (!ctx.opts.legend ||
            ctx.opts.legend.show === false ||
            items.length === 0) {
            return;
        }

        const legend = document.createElement('div');

        legend.className = 'tech-chart-legend';

        items.forEach(function (item) {

            const button = document.createElement('button');

            button.type = 'button';
            button.className = 'tech-chart-legend-item' +
                (hiddenSet.has(item.key) ? ' is-off' : '');

            button.innerHTML =
                '<span class="tech-chart-legend-swatch" style="background:' +
                Tech.escapeHtml(item.color) + '"></span>' +
                '<span class="tech-chart-legend-label">' +
                Tech.escapeHtml(item.label) + '</span>';

            button.addEventListener('click', function () {
                toggle(item.key);
            });

            legend.appendChild(button);
        });

        if (ctx.opts.legend.position === 'top') {
            ctx.root.insertBefore(legend, ctx.canvas);
        }
        else {
            ctx.root.appendChild(legend);
        }
    }

    function seriesLegend(ctx, list) {

        drawLegend(ctx,
            list.map(function (series) {
                return {
                    key: series.index,
                    label: series.name,
                    color: series.color
                };
            }),
            ctx.instance.hiddenSeries,
            function (key) {
                toggleSet(ctx.instance.hiddenSeries, key);
                ctx.instance.render();
            });
    }

    function sliceLegend(ctx, items) {

        drawLegend(ctx, items, ctx.instance.hiddenSlices, function (key) {
            toggleSet(ctx.instance.hiddenSlices, key);
            ctx.instance.render();
        });
    }

    function showEmpty(ctx, message) {

        removeLegend(ctx.root);

        if (ctx.svg && ctx.svg.parentElement) {
            ctx.svg.remove();
        }

        const host = document.createElement('div');

        host.className = 'tech-chart-empty';
        host.textContent = message ||
            ctx.opts.emptyText ||
            'داده‌ای برای نمایش وجود ندارد.';

        ctx.canvas.appendChild(host);
    }

    function positionTip(tip, event) {

        const host = tip.parentElement;

        if (!host) return;

        const rect = host.getBoundingClientRect();

        let x = event.clientX - rect.left + 14;
        let y = event.clientY - rect.top - 12;

        if (x + tip.offsetWidth > rect.width - 6) {
            x = event.clientX - rect.left - tip.offsetWidth - 14;
        }

        if (x < 6) x = 6;
        if (y < 6) y = 6;

        if (y + tip.offsetHeight > rect.height - 6) {
            y = rect.height - tip.offsetHeight - 6;
        }

        tip.style.transform =
            'translate(' + Math.round(x) + 'px,' + Math.round(y) + 'px)';
    }

    function bindTip(node, tip, html) {

        node.style.cursor = 'pointer';

        node.addEventListener('pointerenter', function (event) {

            tip.innerHTML = typeof html === 'function' ? html() : html;
            tip.classList.add('is-visible');
            positionTip(tip, event);
        });

        node.addEventListener('pointermove', function (event) {
            positionTip(tip, event);
        });

        node.addEventListener('pointerleave', function () {
            tip.classList.remove('is-visible');
        });
    }

    function tipHtml(title, rows) {

        let html = '<div class="tech-chart-tip-title">' +
            Tech.escapeHtml(title || '') + '</div>';

        rows.forEach(function (row) {

            html += '<div class="tech-chart-tip-row">';

            if (row.color) {
                html += '<span class="tech-chart-tip-swatch" style="background:' +
                    Tech.escapeHtml(row.color) + '"></span>';
            }

            html += '<span class="tech-chart-tip-name">' +
                Tech.escapeHtml(row.name || '') + '</span>' +
                '<strong class="tech-chart-tip-value">' +
                Tech.escapeHtml(row.value === null ||
                    row.value === undefined ? '' : row.value) +
                '</strong></div>';
        });

        return html;
    }

    function resolveCategories(ctx, series) {

        let length = ctx.categories.length;

        series.forEach(function (item) {
            if (item.data.length > length) length = item.data.length;
            if (item.candles.length > length) length = item.candles.length;
        });

        const categories = [];

        for (let index = 0; index < length; index++) {
            categories.push(ctx.categories[index] ||
                String(index + 1));
        }

        return categories;
    }

    function visibleSeries(ctx) {

        return ctx.series.filter(function (item) {
            return !ctx.instance.hiddenSeries.has(item.index);
        });
    }

    function cartesianExtent(ctx, series, stacked) {

        let min = Infinity;
        let max = -Infinity;

        if (stacked) {

            let length = 0;

            series.forEach(function (item) {
                length = Math.max(length, item.data.length);
            });

            for (let index = 0; index < length; index++) {

                let positive = 0;
                let negative = 0;

                series.forEach(function (item) {

                    const value = num(item.data[index]);

                    if (value === null) return;

                    if (BAR_TYPES[item.type]) {
                        if (value >= 0) positive += value;
                        else negative += value;
                    }
                    else {
                        min = Math.min(min, value);
                        max = Math.max(max, value);
                    }
                });

                min = Math.min(min, negative);
                max = Math.max(max, positive);
            }
        }
        else {

            series.forEach(function (item) {

                item.data.forEach(function (raw) {

                    const value = num(raw);

                    if (value === null) return;

                    min = Math.min(min, value);
                    max = Math.max(max, value);
                });
            });
        }

        if (!Number.isFinite(min) || !Number.isFinite(max)) {
            min = 0;
            max = 1;
        }

        return { min: min, max: max };
    }

    function addGradient(ctx, color, key) {

        if (!ctx.defs) {
            ctx.defs = create('defs', null, ctx.svg);
        }

        const id = 'tech-chart-grad-' + ctx.uid + '-' + key;

        const gradient = create('linearGradient', {
            id: id,
            x1: '0',
            y1: '0',
            x2: '0',
            y2: '1'
        }, ctx.defs);

        create('stop', {
            offset: '0%',
            'stop-color': color,
            'stop-opacity': '.32'
        }, gradient);

        create('stop', {
            offset: '100%',
            'stop-color': color,
            'stop-opacity': '.02'
        }, gradient);

        return 'url(#' + id + ')';
    }

    function findSegments(points) {

        const segments = [];
        let current = [];

        points.forEach(function (point) {

            if (point === null) {

                if (current.length > 0) segments.push(current);

                current = [];
                return;
            }

            current.push(point);
        });

        if (current.length > 0) segments.push(current);

        return segments;
    }

    function dataLabel(ctx, parent, x, y, value, options) {

        if (!ctx.opts.dataLabels) return;

        options = options || {};

        text(parent, value, {
            x: round(x),
            y: round(y),
            'text-anchor': options.anchor || 'middle',
            'font-size': options.size || 10,
            class: 'tech-chart-datalabel' +
                (options.color === '#fff' ? ' tech-chart-oncolor' : '')
        });
    }

    function drawnBar(ctx, parent, x, y, width, height, color, opacity) {

        const rect = create('rect', {
            x: round(x),
            y: round(y),
            width: Math.max(1, round(width)),
            height: Math.max(1, round(height)),
            rx: 3,
            fill: color,
            'fill-opacity': opacity === null ||
                opacity === undefined ? 1 : opacity
        }, parent);

        return rect;
    }

    function drawCartesian(ctx) {

        const opts = ctx.opts;
        const allSeries = visibleSeries(ctx);

        if (allSeries.length === 0) {
            showEmpty(ctx);
            seriesLegend(ctx, ctx.series);
            return;
        }

        const horizontal = opts.horizontal === true ||
            (opts.horizontal !== false && ctx.type === 'bar');

        if (horizontal) {
            drawHorizontalBars(ctx, allSeries);
            return;
        }

        let categories = resolveCategories(ctx, allSeries);

        if (categories.length === 0) {
            showEmpty(ctx);
            return;
        }

        const yAxis = opts.yAxis || {};
        const xAxis = opts.xAxis || {};
        const stacked = opts.stacked === true ||
            allSeries.some(function (item) { return !!item.stack; });

        let waterfall = null;

        if (ctx.type === 'waterfall') {
            waterfall = buildWaterfall(ctx, allSeries[0], categories);
            categories = waterfall.categories;
        }

        const extent = waterfall
            ? { min: waterfall.min, max: waterfall.max }
            : cartesianExtent(ctx, allSeries, stacked);

        const scale = buildScale(extent.min, extent.max, yAxis);
        const ticks = scale.ticks.map(function (value) {
            return ctx.format(value, yAxis.format);
        });

        const axisWidth = Math.max.apply(null,
            ticks.map(function (label) { return estimate(label, 11); }))
            + 14;

        const rotate = Number(xAxis.rotate) || 0;
        const labelHeight = xAxis.labels === false
            ? 8
            : (rotate !== 0 ? 48 : 24);

        const yTitleSpace = yAxis.title && yAxis.show !== false ? 18 : 0;
        const pad = 16;

        const plotLeft = ctx.dir
            ? pad
            : pad + axisWidth + yTitleSpace;

        const plotRight = ctx.width - pad -
            (ctx.dir ? axisWidth + yTitleSpace : 0);

        const plotTop = pad + 10;

        const plotBottom = ctx.height - pad - labelHeight -
            (xAxis.title && xAxis.show !== false ? 18 : 0);

        if (plotRight - plotLeft < 40 || plotBottom - plotTop < 40) {
            showEmpty(ctx);
            return;
        }

        const y = function (value) {

            return plotBottom -
                ((value - scale.min) / (scale.max - scale.min)) *
                (plotBottom - plotTop);
        };

        const slot = (plotRight - plotLeft) / categories.length;

        const center = function (index) {
            return plotLeft + slot * (index + 0.5);
        };

        const plot = create('g', { class: 'tech-chart-plot' }, ctx.svg);

        if (opts.animation !== false) {
            plot.classList.add('tech-chart-animate');
        }

        if (yAxis.show !== false) {

            scale.ticks.forEach(function (tick) {

                const tickY = y(tick);

                if (yAxis.grid !== false) {

                    create('line', {
                        x1: round(plotLeft),
                        y1: round(tickY),
                        x2: round(plotRight),
                        y2: round(tickY),
                        class: 'tech-chart-grid'
                    }, plot);
                }

                if (yAxis.labels !== false) {

                    text(plot, ctx.format(tick, yAxis.format), {
                        x: ctx.dir ? round(plotRight + 8)
                            : round(plotLeft - 8),
                        y: round(tickY + 4),
                        'text-anchor': 'end',
                        'font-size': 11,
                        class: 'tech-chart-axis-label'
                    });
                }
            });

            if (yAxis.title) {

                const titleX = ctx.dir
                    ? ctx.width - 4
                    : 12;

                text(plot, yAxis.title, {
                    x: round(titleX),
                    y: round((plotTop + plotBottom) / 2),
                    'text-anchor': 'middle',
                    transform: 'rotate(-90 ' + round(titleX) + ' ' +
                        round((plotTop + plotBottom) / 2) + ')',
                    'font-size': 11,
                    class: 'tech-chart-axis-title'
                });
            }
        }

        if (xAxis.show !== false && xAxis.labels !== false) {

            categories.forEach(function (category, index) {

                const labelX = center(index);

                const attrs = {
                    x: round(labelX),
                    y: round(plotBottom + 16),
                    'text-anchor': rotate === 0
                        ? 'middle'
                        : textAnchor(ctx.dir, 'left'),
                    'font-size': 11,
                    class: 'tech-chart-axis-label'
                };

                if (rotate !== 0) {
                    attrs.transform = 'rotate(' + (-rotate) + ' ' +
                        round(labelX) + ' ' + round(plotBottom + 16) + ')';
                }

                text(plot, category, attrs);
            });
        }

        if (xAxis.title && xAxis.show !== false) {

            text(plot, xAxis.title, {
                x: round((plotLeft + plotRight) / 2),
                y: round(ctx.height - 4),
                'text-anchor': 'middle',
                'font-size': 11,
                class: 'tech-chart-axis-title'
            });
        }

        if (scale.min < 0 && scale.max > 0) {

            create('line', {
                x1: round(plotLeft),
                y1: round(y(0)),
                x2: round(plotRight),
                y2: round(y(0)),
                class: 'tech-chart-axis-line'
            }, plot);
        }

        if (waterfall) {
            drawWaterfallSeries(ctx, plot, waterfall, categories, center, slot, y, scale);
        }
        else {
            drawVerticalBars(ctx, plot, allSeries, categories, center, slot, y, plotBottom, stacked);
        }

        allSeries.forEach(function (series) {

            if (!LINE_TYPES[series.type]) return;

            drawLineSeries(ctx, plot, series, categories, center, y, plotBottom);
        });

        seriesLegend(ctx, ctx.series);
    }

    function drawVerticalBars(
        ctx, plot, series, categories, center, slot, y, baseline, stacked) {

        const barSeries = series.filter(function (item) {
            return BAR_TYPES[item.type];
        });

        if (barSeries.length === 0) return;

        const groups = [];

        if (stacked) {

            barSeries.forEach(function (item) {

                const key = item.stack || '__default';

                let group = groups.find(function (candidate) {
                    return candidate.key === key;
                });

                if (!group) {

                    group = { key: key, items: [] };
                    groups.push(group);
                }

                group.items.push(item);
            });
        }
        else {

            barSeries.forEach(function (item) {
                groups.push({ key: item.stack || item.index, items: [item] });
            });
        }

        const groupWidth = Math.min(slot * 0.72, 64);
        const gap = groups.length > 1 ? 3 : 0;
        const barWidth = stacked
            ? groupWidth
            : Math.max(2,
                (groupWidth - gap * (groups.length - 1)) / groups.length);

        categories.forEach(function (category, categoryIndex) {

            groups.forEach(function (group, groupIndex) {

                let positive = 0;
                let negative = 0;

                const groupStart = center(categoryIndex) -
                    groupWidth / 2 +
                    groupIndex * (barWidth + gap);

                group.items.forEach(function (item) {

                    const value = num(item.data[categoryIndex]);

                    if (value === null) return;

                    let from;
                    let to;

                    if (stacked) {
                        if (value >= 0) {
                            from = positive;
                            to = positive + value;
                            positive = to;
                        }
                        else {
                            from = negative;
                            to = negative + value;
                            negative = to;
                        }
                    }
                    else {
                        from = 0;
                        to = value;
                    }

                    const top = Math.min(y(from), y(to));

                    const rect = drawnBar(ctx, plot,
                        groupStart,
                        top,
                        barWidth,
                        Math.abs(y(to) - y(from)),
                        item.color,
                        item.opacity);

                    bindTip(rect, ctx.tip, function () {

                        return tipHtml(category, [{
                            name: item.name,
                            color: item.color,
                            value: ctx.format(value, ctx.opts.valueFormat)
                        }]);
                    });

                    if (value !== 0 && barWidth > 22) {

                        dataLabel(ctx, plot,
                            groupStart + barWidth / 2,
                            value >= 0 ? top - 5 : top + Math.abs(y(to) - y(from)) + 12,
                            ctx.format(value, ctx.opts.valueFormat));
                    }
                });
            });
        });
    }

    function drawLineSeries(ctx, plot, series, categories, center, y, baseline) {

        const mode = series.type === 'step'
            ? 'step'
            : (series.type === 'spline' || series.type === 'areaspline' ||
                ctx.opts.smooth === true ? 'smooth' : 'line');

        const showArea = series.type === 'area' ||
            series.type === 'areaspline' ||
            ctx.opts.area === true;

        const points = series.data.map(function (raw, index) {

            const value = num(raw);

            if (value === null) return null;

            return {
                x: center(index),
                y: y(value),
                value: value,
                index: index
            };
        });

        const segments = findSegments(points);

        segments.forEach(function (segment, segmentIndex) {

            if (showArea) {

                create('path', {
                    d: areaPath(segment, mode, baseline),
                    fill: addGradient(ctx, series.color,
                        series.index + '-' + segmentIndex),
                    stroke: 'none',
                    'fill-opacity': series.opacity === null ||
                        series.opacity === undefined
                        ? 1
                        : series.opacity
                }, plot);
            }

            create('path', {
                d: linePath(segment, mode),
                fill: 'none',
                stroke: series.color,
                'stroke-width': 2.25,
                'stroke-linecap': 'round',
                'stroke-linejoin': 'round',
                'stroke-dasharray': series.dash ? '6 5' : null,
                'stroke-opacity': series.opacity === null ||
                    series.opacity === undefined
                    ? 1
                    : series.opacity
            }, plot);
        });

        points.forEach(function (point) {

            if (point === null) return;

            const visible = create('circle', {
                cx: round(point.x),
                cy: round(point.y),
                r: 3,
                fill: 'var(--tech-surface)',
                stroke: series.color,
                'stroke-width': 2
            }, plot);

            visible.style.pointerEvents = 'none';

            const hit = create('circle', {
                cx: round(point.x),
                cy: round(point.y),
                r: 10,
                fill: 'transparent'
            }, plot);

            bindTip(hit, ctx.tip, function () {

                return tipHtml(categories[point.index] || '', [{
                    name: series.name,
                    color: series.color,
                    value: ctx.format(point.value, ctx.opts.valueFormat)
                }]);
            });

            dataLabel(ctx, plot, point.x, point.y - 8,
                ctx.format(point.value, ctx.opts.valueFormat));
        });
    }

    function buildWaterfall(ctx, series, categories) {

        if (!series) {
            return {
                categories: categories,
                steps: [],
                min: 0,
                max: 1
            };
        }

        const steps = [];
        const labels = categories.slice();
        let running = 0;
        let min = 0;
        let max = 0;

        series.data.forEach(function (raw, index) {

            const value = num(raw);

            if (value === null) return;

            const start = running;
            const end = running + value;

            running = end;

            steps.push({
                index: index,
                label: categories[index] || String(index + 1),
                start: start,
                end: end,
                value: value,
                total: false
            });

            min = Math.min(min, start, end);
            max = Math.max(max, start, end);
        });

        const showTotal = !ctx.opts.waterfall ||
            ctx.opts.waterfall.showTotal !== false;

        if (showTotal && steps.length > 0) {

            labels.push('جمع');

            steps.push({
                index: steps.length,
                label: 'جمع',
                start: 0,
                end: running,
                value: running,
                total: true
            });

            min = Math.min(min, 0, running);
            max = Math.max(max, 0, running);
        }

        return {
            categories: labels,
            steps: steps,
            min: min,
            max: max
        };
    }

    function drawWaterfallSeries(
        ctx, plot, waterfall, categories, center, slot, y, scale) {

        const width = Math.min(slot * 0.62, 54);

        waterfall.steps.forEach(function (step, index) {

            let color = step.total
                ? cssVar(ctx.root, '--tech-chart-total', '#64748b')
                : step.value >= 0
                    ? ctx.palette[0]
                    : cssVar(ctx.root, '--tech-chart-negative', '#dc2626');

            const top = Math.min(y(step.start), y(step.end));

            const rect = drawnBar(ctx, plot,
                center(index) - width / 2,
                top,
                width,
                Math.abs(y(step.end) - y(step.start)),
                color,
                1);

            bindTip(rect, ctx.tip, function () {

                return tipHtml(step.label, [{
                    name: step.total ? 'جمع کل' : 'تغییر',
                    color: color,
                    value: ctx.format(step.value, ctx.opts.valueFormat)
                }, {
                    name: 'تجمعی',
                    value: ctx.format(step.end, ctx.opts.valueFormat)
                }]);
            });

            dataLabel(ctx, plot,
                center(index),
                top - 5,
                ctx.format(step.value, ctx.opts.valueFormat));

            if (index < waterfall.steps.length - 1) {

                const next = waterfall.steps[index + 1];

                create('line', {
                    x1: round(center(index) + width / 2),
                    y1: round(y(step.end)),
                    x2: round(center(index + 1) - width / 2),
                    y2: round(y(next.start)),
                    class: 'tech-chart-connector'
                }, plot);
            }
        });
    }

    function drawHorizontalBars(ctx, series) {

        const opts = ctx.opts;
        const xAxis = opts.xAxis || {};
        const yAxis = opts.yAxis || {};
        const barSeries = series.filter(function (item) {
            return BAR_TYPES[item.type];
        });

        if (barSeries.length === 0) {
            showEmpty(ctx);
            return;
        }

        let categories = resolveCategories(ctx, barSeries);

        if (categories.length === 0) {
            showEmpty(ctx);
            return;
        }

        const stacked = opts.stacked === true ||
            barSeries.some(function (item) { return !!item.stack; });

        const extent = cartesianExtent(ctx, barSeries, stacked);
        const scale = buildScale(extent.min, extent.max, xAxis);

        const tickLabels = scale.ticks.map(function (value) {
            return ctx.format(value, xAxis.format);
        });

        const tickWidth = Math.max.apply(null,
            tickLabels.map(function (label) { return estimate(label, 11); }));

        const labelWidth = Math.min(
            ctx.width * 0.32,
            Math.max.apply(null, categories.map(function (category) {
                return estimate(category, 11);
            })) + 14);

        const pad = 16;
        const bottomSpace = xAxis.labels === false
            ? 8
            : 26 + (xAxis.title ? 18 : 0);

        const plotLeft = pad + labelWidth;
        const plotRight = ctx.width - pad -
            (ctx.dir ? 0 : Math.min(tickWidth * 0.4, 30));

        const plotTop = pad;
        const plotBottom = ctx.height - pad - bottomSpace;

        if (plotRight - plotLeft < 40 || plotBottom - plotTop < 40) {
            showEmpty(ctx);
            return;
        }

        const x = function (value) {

            return plotLeft +
                ((value - scale.min) / (scale.max - scale.min)) *
                (plotRight - plotLeft);
        };

        const slot = (plotBottom - plotTop) / categories.length;

        const plot = create('g', { class: 'tech-chart-plot' }, ctx.svg);

        if (opts.animation !== false) {
            plot.classList.add('tech-chart-animate');
        }

        if (xAxis.show !== false) {

            scale.ticks.forEach(function (tick) {

                if (xAxis.grid !== false) {

                    create('line', {
                        x1: round(x(tick)),
                        y1: round(plotTop),
                        x2: round(x(tick)),
                        y2: round(plotBottom),
                        class: 'tech-chart-grid'
                    }, plot);
                }

                if (xAxis.labels !== false) {

                    text(plot, ctx.format(tick, xAxis.format), {
                        x: round(x(tick)),
                        y: round(plotBottom + 16),
                        'text-anchor': 'middle',
                        'font-size': 11,
                        class: 'tech-chart-axis-label'
                    });
                }
            });

            if (xAxis.title) {

                text(plot, xAxis.title, {
                    x: round((plotLeft + plotRight) / 2),
                    y: round(ctx.height - 4),
                    'text-anchor': 'middle',
                    'font-size': 11,
                    class: 'tech-chart-axis-title'
                });
            }
        }

        if (scale.min < 0 && scale.max > 0) {

            create('line', {
                x1: round(x(0)),
                y1: round(plotTop),
                x2: round(x(0)),
                y2: round(plotBottom),
                class: 'tech-chart-axis-line'
            }, plot);
        }

        categories.forEach(function (category, index) {

            text(plot, category, {
                x: round(plotLeft - 8),
                y: round(plotTop + slot * (index + 0.5) + 4),
                'text-anchor': textAnchor(ctx.dir, 'left'),
                'font-size': 11,
                class: 'tech-chart-axis-label'
            });
        });

        const groups = [];

        if (stacked) {

            barSeries.forEach(function (item) {

                const key = item.stack || '__default';

                let group = groups.find(function (candidate) {
                    return candidate.key === key;
                });

                if (!group) {
                    group = { key: key, items: [] };
                    groups.push(group);
                }

                group.items.push(item);
            });
        }
        else {

            barSeries.forEach(function (item) {
                groups.push({ key: item.stack || item.index, items: [item] });
            });
        }

        const groupHeight = Math.min(slot * 0.7, 46);
        const gap = groups.length > 1 ? 3 : 0;
        const barHeight = stacked
            ? groupHeight
            : Math.max(2,
                (groupHeight - gap * (groups.length - 1)) / groups.length);

        categories.forEach(function (category, categoryIndex) {

            groups.forEach(function (group, groupIndex) {

                let positive = 0;
                let negative = 0;

                const top = plotTop + slot * categoryIndex +
                    (slot - groupHeight) / 2 +
                    groupIndex * (barHeight + gap);

                group.items.forEach(function (item) {

                    const value = num(item.data[categoryIndex]);

                    if (value === null) return;

                    let from;
                    let to;

                    if (stacked) {
                        if (value >= 0) {
                            from = positive;
                            to = positive + value;
                            positive = to;
                        }
                        else {
                            from = negative;
                            to = negative + value;
                            negative = to;
                        }
                    }
                    else {
                        from = 0;
                        to = value;
                    }

                    const left = Math.min(x(from), x(to));

                    const rect = drawnBar(ctx, plot,
                        left,
                        top,
                        Math.abs(x(to) - x(from)),
                        barHeight,
                        item.color,
                        item.opacity);

                    bindTip(rect, ctx.tip, function () {

                        return tipHtml(category, [{
                            name: item.name,
                            color: item.color,
                            value: ctx.format(value, ctx.opts.valueFormat)
                        }]);
                    });
                });
            });
        });

        seriesLegend(ctx, ctx.series.filter(function (item) {
            return BAR_TYPES[item.type];
        }));
    }

    function drawScatter(ctx, allSeries) {

        const bubble = ctx.type === 'bubble';
        const points = [];

        allSeries.forEach(function (series) {

            const seriesPoints = [];

            if (series.points.length > 0) {
                series.points.forEach(function (point) {
                    seriesPoints.push({
                        x: num(point.x),
                        y: num(point.y),
                        r: num(point.r),
                        label: point.label
                    });
                });
            }
            else {
                series.data.forEach(function (raw, index) {
                    seriesPoints.push({
                        x: index + 1,
                        y: num(raw),
                        r: null,
                        label: null
                    });
                });
            }

            seriesPoints.forEach(function (point) {
                if (point.x === null || point.y === null) return;

                points.push({
                    series: series,
                    x: point.x,
                    y: point.y,
                    r: point.r,
                    label: point.label
                });
            });
        });

        if (points.length === 0) {
            showEmpty(ctx);
            seriesLegend(ctx, ctx.series);
            return;
        }

        const opts = ctx.opts;
        const xAxis = opts.xAxis || {};
        const yAxis = opts.yAxis || {};
        const xScale = buildScale(
            Math.min.apply(null, points.map(function (point) { return point.x; })),
            Math.max.apply(null, points.map(function (point) { return point.x; })),
            xAxis);
        const yScale = buildScale(
            Math.min.apply(null, points.map(function (point) { return point.y; })),
            Math.max.apply(null, points.map(function (point) { return point.y; })),
            yAxis);

        const pad = 18;
        const axisWidth = Math.max.apply(null,
            yScale.ticks.map(function (value) {
                return estimate(ctx.format(value, yAxis.format), 11);
            })) + 14;

        const plotLeft = ctx.dir ? pad : pad + axisWidth;
        const plotRight = ctx.width - pad - (ctx.dir ? axisWidth : 0);
        const plotTop = pad + 8;
        const plotBottom = ctx.height - pad - 26;

        if (plotRight - plotLeft < 40) {
            showEmpty(ctx);
            return;
        }

        const x = function (value) {
            return plotLeft +
                ((value - xScale.min) / (xScale.max - xScale.min)) *
                (plotRight - plotLeft);
        };

        const y = function (value) {
            return plotBottom -
                ((value - yScale.min) / (yScale.max - yScale.min)) *
                (plotBottom - plotTop);
        };

        const plot = create('g', { class: 'tech-chart-plot' }, ctx.svg);

        if (opts.animation !== false) {
            plot.classList.add('tech-chart-animate');
        }

        if (yAxis.show !== false) {

            yScale.ticks.forEach(function (tick) {

                if (yAxis.grid !== false) {

                    create('line', {
                        x1: round(plotLeft),
                        y1: round(y(tick)),
                        x2: round(plotRight),
                        y2: round(y(tick)),
                        class: 'tech-chart-grid'
                    }, plot);
                }

                if (yAxis.labels !== false) {

                    text(plot, ctx.format(tick, yAxis.format), {
                        x: ctx.dir ? round(plotRight + 8)
                            : round(plotLeft - 8),
                        y: round(y(tick) + 4),
                        'text-anchor': 'end',
                        'font-size': 11,
                        class: 'tech-chart-axis-label'
                    });
                }
            });
        }

        if (xAxis.show !== false) {

            xScale.ticks.forEach(function (tick) {

                if (xAxis.grid !== false) {

                    create('line', {
                        x1: round(x(tick)),
                        y1: round(plotTop),
                        x2: round(x(tick)),
                        y2: round(plotBottom),
                        class: 'tech-chart-grid'
                    }, plot);
                }

                if (xAxis.labels !== false) {

                    text(plot, ctx.format(tick, xAxis.format), {
                        x: round(x(tick)),
                        y: round(plotBottom + 16),
                        'text-anchor': 'middle',
                        'font-size': 11,
                        class: 'tech-chart-axis-label'
                    });
                }
            });
        }

        const maxRadius = bubble
            ? Math.max.apply(null, points.map(function (point) {
                return point.r === null ? 1 : point.r;
            }))
            : 1;

        points.forEach(function (point) {

            const radius = bubble
                ? 4 + Math.sqrt(Math.max(0, point.r === null ? 1 : point.r) /
                    Math.max(1, maxRadius)) * 18
                : 4;

            const dot = create('circle', {
                cx: round(x(point.x)),
                cy: round(y(point.y)),
                r: round(radius),
                fill: point.series.color,
                'fill-opacity': '.72',
                stroke: point.series.color,
                'stroke-width': 1
            }, plot);

            bindTip(dot, ctx.tip, function () {

                return tipHtml(point.label ||
                    ctx.format(point.x, xAxis.format), [{
                    name: point.series.name,
                    color: point.series.color,
                    value: ctx.format(point.y, yAxis.format ||
                        ctx.opts.valueFormat)
                }, {
                    name: 'X',
                    value: ctx.format(point.x, xAxis.format)
                }]);
            });
        });

        seriesLegend(ctx, ctx.series);
    }

    function drawCandles(ctx, allSeries) {

        const series = allSeries[0];

        if (!series || series.candles.length === 0) {
            showEmpty(ctx);
            return;
        }

        const categories = resolveCategories(ctx, allSeries);
        const yAxis = ctx.opts.yAxis || {};
        let min = Infinity;
        let max = -Infinity;

        series.candles.forEach(function (candle) {
            min = Math.min(min, candle.low);
            max = Math.max(max, candle.high);
        });

        if (!Number.isFinite(min)) {
            showEmpty(ctx);
            return;
        }

        const scale = buildScale(min, max, yAxis);

        const ticks = scale.ticks.map(function (value) {
            return ctx.format(value, yAxis.format);
        });

        const axisWidth = Math.max.apply(null,
            ticks.map(function (label) { return estimate(label, 11); }))
            + 14;

        const pad = 16;
        const plotLeft = ctx.dir ? pad : pad + axisWidth;
        const plotRight = ctx.width - pad - (ctx.dir ? axisWidth : 0);
        const plotTop = pad + 10;
        const plotBottom = ctx.height - pad - 26;

        const y = function (value) {
            return plotBottom -
                ((value - scale.min) / (scale.max - scale.min)) *
                (plotBottom - plotTop);
        };

        const slot = (plotRight - plotLeft) / categories.length;
        const center = function (index) {
            return plotLeft + slot * (index + 0.5);
        };

        const plot = create('g', { class: 'tech-chart-plot' }, ctx.svg);

        if (ctx.opts.animation !== false) {
            plot.classList.add('tech-chart-animate');
        }

        scale.ticks.forEach(function (tick) {

            if (yAxis.grid !== false) {

                create('line', {
                    x1: round(plotLeft),
                    y1: round(y(tick)),
                    x2: round(plotRight),
                    y2: round(y(tick)),
                    class: 'tech-chart-grid'
                }, plot);
            }

            if (yAxis.labels !== false) {

                text(plot, ctx.format(tick, yAxis.format), {
                    x: ctx.dir ? round(plotRight + 8)
                        : round(plotLeft - 8),
                    y: round(y(tick) + 4),
                    'text-anchor': 'end',
                    'font-size': 11,
                    class: 'tech-chart-axis-label'
                });
            }
        });

        categories.forEach(function (category, index) {

            if (index % Math.ceil(categories.length / 12) !== 0) return;

            text(plot, category, {
                x: round(center(index)),
                y: round(plotBottom + 16),
                'text-anchor': 'middle',
                'font-size': 11,
                class: 'tech-chart-axis-label'
            });
        });

        const upColor = cssVar(ctx.root, '--tech-chart-positive', '#16a34a');
        const downColor = cssVar(ctx.root, '--tech-chart-negative', '#dc2626');
        const width = Math.min(slot * 0.56, 26);

        series.candles.forEach(function (candle, index) {

            const color = candle.close >= candle.open ? upColor : downColor;
            const cx = center(index);

            create('line', {
                x1: round(cx),
                y1: round(y(candle.high)),
                x2: round(cx),
                y2: round(y(candle.low)),
                stroke: color,
                'stroke-width': 1.4
            }, plot);

            const top = Math.min(y(candle.open), y(candle.close));
            const body = create('rect', {
                x: round(cx - width / 2),
                y: round(top),
                width: round(width),
                height: Math.max(1.5,
                    Math.abs(y(candle.close) - y(candle.open))),
                rx: 1.5,
                fill: color
            }, plot);

            bindTip(body, ctx.tip, function () {

                const format = ctx.opts.valueFormat;
                const category = categories[index] || '';

                return tipHtml(category, [
                    { name: 'باز', color: color, value: ctx.format(candle.open, format) },
                    { name: 'بالا', value: ctx.format(candle.high, format) },
                    { name: 'پایین', value: ctx.format(candle.low, format) },
                    { name: 'بسته', value: ctx.format(candle.close, format) }
                ]);
            });
        });
    }

    function drawPie(ctx, allSeries) {

        const variant = ctx.type;
        const series = allSeries[0];

        if (!series) {
            showEmpty(ctx);
            return;
        }

        let categories = ctx.categories.slice();

        if (categories.length < series.data.length) {
            for (let index = categories.length;
                index < series.data.length;
                index++) {

                categories.push(String(index + 1));
            }
        }

        const hidden = ctx.instance.hiddenSlices;

        const allItems = series.data.map(function (raw, index) {

            return {
                key: index,
                label: categories[index] || String(index + 1),
                value: Math.max(0, num(raw) || 0),
                color: ctx.palette[index % ctx.palette.length]
            };
        }).filter(function (item) {
            return item.value > 0;
        });

        const items = allItems.filter(function (item) {
            return !hidden.has(item.key);
        });

        if (items.length === 0) {
            showEmpty(ctx);
            sliceLegend(ctx, allItems);
            return;
        }

        const total = sumOf(items.map(function (item) { return item.value; }));

        if (total <= 0) {
            showEmpty(ctx);
            return;
        }

        const pad = 20;
        const cx = ctx.width / 2;
        const cy = (ctx.height + pad) / 2 - 4;
        const radius = Math.max(40,
            Math.min(ctx.width, ctx.height) / 2 - 28);

        const holeRatio = variant === 'donut'
            ? clamp(Number(ctx.opts.donut && ctx.opts.donut.hole) || 0.6,
                0, 0.9)
            : 0;

        const inner = radius * holeRatio;

        const plot = create('g', { class: 'tech-chart-plot' }, ctx.svg);

        if (ctx.opts.animation !== false) {
            plot.classList.add('tech-chart-animate');
        }

        let angle = -90;

        if (variant === 'polar') {

            const maxValue = Math.max.apply(null,
                items.map(function (item) { return item.value; }));

            for (let ring = 1; ring <= 4; ring++) {

                create('circle', {
                    cx: round(cx),
                    cy: round(cy),
                    r: round(radius * ring / 4),
                    fill: 'none',
                    class: 'tech-chart-grid'
                }, plot);
            }

            items.forEach(function (item) {
                item.sweep = 360 / items.length;
                item.radius = radius *
                    Math.sqrt(item.value / Math.max(1, maxValue));
            });
        }
        else {
            items.forEach(function (item) {
                item.sweep = item.value / total * 360;
            });
        }

        items.forEach(function (item) {

            const start = angle;
            const end = angle + item.sweep;

            angle = end;

            const path = variant === 'polar'
                ? wedgeSlice(cx, cy, item.radius, start, end)
                : holeRatio > 0
                    ? ringSlice(cx, cy, inner, radius, start, end)
                    : wedgeSlice(cx, cy, radius, start, end);

            const slice = create('path', {
                d: path,
                fill: item.color,
                stroke: 'var(--tech-surface)',
                'stroke-width': 2
            }, plot);

            bindTip(slice, ctx.tip, function () {

                return tipHtml(item.label, [{
                    name: 'مقدار',
                    color: item.color,
                    value: ctx.format(item.value, ctx.opts.valueFormat)
                }, {
                    name: 'سهم',
                    value: formatValue(
                        item.value / total * 100, 'percent', ctx.locale)
                }]);
            });

            if (item.sweep >= 12) {

                const middle = start + item.sweep / 2;
                const labelRadius = variant === 'polar'
                    ? item.radius * 0.62
                    : holeRatio > 0
                        ? (inner + radius) / 2
                        : radius * 0.64;

                const position = polar(cx, cy, labelRadius, middle);

                dataLabel(ctx, plot, position.x, position.y + 4,
                    formatValue(item.value / total * 100, 'percent', ctx.locale),
                    { color: '#fff', size: 11 });
            }
        });

        if (holeRatio > 0) {

            const centerText = ctx.opts.donut &&
                ctx.opts.donut.centerText;

            text(plot, centerText || ctx.format(total, ctx.opts.valueFormat), {
                x: round(cx),
                y: round(cy + 6),
                'text-anchor': 'middle',
                'font-size': 22,
                'font-weight': 700,
                class: 'tech-chart-center-value'
            });

            text(plot, centerText ? 'مجموع' : (series.name || ''), {
                x: round(cx),
                y: round(cy + 28),
                'text-anchor': 'middle',
                'font-size': 11,
                class: 'tech-chart-axis-label'
            });
        }

        sliceLegend(ctx, allItems.map(function (item) {
            return {
                key: item.key,
                label: item.label,
                color: item.color
            };
        }));
    }

    function drawRadialBar(ctx, allSeries) {

        const series = allSeries;

        if (series.length === 0) {
            showEmpty(ctx);
            seriesLegend(ctx, ctx.series);
            return;
        }

        const values = series.map(function (item) {
            return num(item.data[0]) || 0;
        });

        const maxValue = ctx.opts.yAxis &&
            ctx.opts.yAxis.max !== null &&
            ctx.opts.yAxis.max !== undefined
            ? Number(ctx.opts.yAxis.max)
            : Math.max(100, Math.max.apply(null, values) * 1.15);

        const pad = 22;
        const cx = ctx.width / 2;
        const cy = ctx.height / 2;
        const radius = Math.max(40,
            Math.min(ctx.width, ctx.height) / 2 - pad);

        const gap = Math.min(18, radius / Math.max(2, series.length + 0.5));
        const thickness = Math.max(8, Math.min(18, gap * 0.62));

        const plot = create('g', { class: 'tech-chart-plot' }, ctx.svg);

        if (ctx.opts.animation !== false) {
            plot.classList.add('tech-chart-animate');
        }

        series.forEach(function (item, index) {

            const ringRadius = radius - index * gap;
            const value = values[index];
            const ratio = clamp(value / Math.max(1, maxValue), 0, 1);

            create('circle', {
                cx: round(cx),
                cy: round(cy),
                r: round(ringRadius),
                fill: 'none',
                stroke: 'var(--tech-border)',
                'stroke-width': round(thickness)
            }, plot);

            if (ratio > 0) {

                create('path', {
                    d: arcPath(cx, cy, ringRadius, -90, -90 + ratio * 360),
                    fill: 'none',
                    stroke: item.color,
                    'stroke-width': round(thickness),
                    'stroke-linecap': 'round'
                }, plot);
            }

            const dot = create('circle', {
                cx: round(cx),
                cy: round(cy),
                r: round(ringRadius),
                fill: 'transparent',
                'stroke-width': round(thickness)
            }, plot);

            dot.style.cursor = 'pointer';

            bindTip(dot, ctx.tip, function () {

                return tipHtml(item.name, [{
                    name: 'مقدار',
                    color: item.color,
                    value: ctx.format(value, ctx.opts.valueFormat)
                }, {
                    name: 'درصد',
                    value: formatValue(ratio * 100, 'percent', ctx.locale)
                }]);
            });
        });

        const focus = series[0];
        const focusValue = values[0];

        text(plot, ctx.format(focusValue, ctx.opts.valueFormat), {
            x: round(cx),
            y: round(cy + 4),
            'text-anchor': 'middle',
            'font-size': 24,
            'font-weight': 700,
            class: 'tech-chart-center-value'
        });

        text(plot, focus.name, {
            x: round(cx),
            y: round(cy + 26),
            'text-anchor': 'middle',
            'font-size': 11,
            class: 'tech-chart-axis-label'
        });

        seriesLegend(ctx, ctx.series);
    }

    function drawGauge(ctx, allSeries) {

        const series = allSeries[0];
        const value = series ? num(series.data[0]) : null;

        if (value === null) {
            showEmpty(ctx);
            return;
        }

        const gauge = ctx.opts.gauge || {};
        const min = Number.isFinite(Number(gauge.min)) ? Number(gauge.min) : 0;
        const max = Number.isFinite(Number(gauge.max)) ? Number(gauge.max) : 100;
        const ratio = clamp((value - min) / Math.max(1e-9, max - min), 0, 1);

        const pad = 24;
        const cx = ctx.width / 2;
        const cy = ctx.height * 0.74;
        const radius = Math.max(50,
            Math.min(ctx.width * 0.42, cy - pad, ctx.height * 0.56));
        const thickness = Math.max(10, radius * 0.12);

        const plot = create('g', { class: 'tech-chart-plot' }, ctx.svg);

        if (ctx.opts.animation !== false) {
            plot.classList.add('tech-chart-animate');
        }

        create('path', {
            d: arcPath(cx, cy, radius, 180, 360),
            fill: 'none',
            stroke: 'var(--tech-border)',
            'stroke-width': round(thickness),
            'stroke-linecap': 'round'
        }, plot);

        const bands = [
            { from: 0, to: 1 / 3, color: cssVar(ctx.root, '--tech-chart-positive', '#16a34a') },
            { from: 1 / 3, to: 2 / 3, color: cssVar(ctx.root, '--tech-chart-warning', '#f59e0b') },
            { from: 2 / 3, to: 1, color: cssVar(ctx.root, '--tech-chart-negative', '#dc2626') }
        ];

        bands.forEach(function (band) {

            create('path', {
                d: arcPath(cx, cy, radius,
                    180 + band.from * 180,
                    180 + band.to * 180),
                fill: 'none',
                stroke: band.color,
                'stroke-width': round(thickness),
                'stroke-opacity': '.22',
                'stroke-linecap': 'butt'
            }, plot);
        });

        const valueEnd = 180 + ratio * 180;

        if (ratio > 0) {

            create('path', {
                d: arcPath(cx, cy, radius, 180, valueEnd),
                fill: 'none',
                stroke: series.color,
                'stroke-width': round(thickness),
                'stroke-linecap': 'round'
            }, plot);
        }

        text(plot, ctx.format(min, ctx.opts.valueFormat), {
            x: round(cx - radius),
            y: round(cy + 24),
            'text-anchor': 'middle',
            'font-size': 11,
            class: 'tech-chart-axis-label'
        });

        text(plot, ctx.format(max, ctx.opts.valueFormat), {
            x: round(cx + radius),
            y: round(cy + 24),
            'text-anchor': 'middle',
            'font-size': 11,
            class: 'tech-chart-axis-label'
        });

        if (gauge.target !== null && gauge.target !== undefined) {

            const targetRatio = clamp(
                (Number(gauge.target) - min) / Math.max(1e-9, max - min),
                0, 1);
            const target = polar(cx, cy, radius, 180 + targetRatio * 180);

            create('line', {
                x1: round(target.x),
                y1: round(target.y),
                x2: round(polar(cx, cy, radius - thickness, 180 + targetRatio * 180).x),
                y2: round(polar(cx, cy, radius - thickness, 180 + targetRatio * 180).y),
                stroke: 'var(--tech-text)',
                'stroke-width': 2,
                'stroke-linecap': 'round'
            }, plot);
        }

        const needle = polar(cx, cy, radius - thickness / 2, valueEnd);

        create('line', {
            x1: round(cx),
            y1: round(cy),
            x2: round(needle.x),
            y2: round(needle.y),
            stroke: 'var(--tech-text)',
            'stroke-width': 2.5,
            'stroke-linecap': 'round'
        }, plot);

        create('circle', {
            cx: round(cx),
            cy: round(cy),
            r: 6,
            fill: series.color
        }, plot);

        text(plot, ctx.format(value, ctx.opts.valueFormat), {
            x: round(cx),
            y: round(cy - 18),
            'text-anchor': 'middle',
            'font-size': 26,
            'font-weight': 700,
            class: 'tech-chart-center-value'
        });

        text(plot, series.name, {
            x: round(cx),
            y: round(cy + 4),
            'text-anchor': 'middle',
            'font-size': 11,
            class: 'tech-chart-axis-label'
        });
    }

    function drawRadar(ctx, allSeries) {

        const categories = ctx.categories.length
            ? ctx.categories
            : resolveCategories(ctx, allSeries);

        if (categories.length < 3 || allSeries.length === 0) {
            showEmpty(ctx);
            seriesLegend(ctx, ctx.series);
            return;
        }

        const yAxis = ctx.opts.yAxis || {};
        const maxValue = yAxis.max !== null && yAxis.max !== undefined
            ? Number(yAxis.max)
            : (maxOf(allSeries.flatMap(function (item) {
                return item.data.map(num);
            })) || 1);

        const pad = 42;
        const cx = ctx.width / 2;
        const cy = ctx.height / 2;
        const radius = Math.max(40,
            Math.min(cx, cy) - pad);

        const angleStep = 360 / categories.length;
        const startAngle = -90;

        const plot = create('g', { class: 'tech-chart-plot' }, ctx.svg);

        if (ctx.opts.animation !== false) {
            plot.classList.add('tech-chart-animate');
        }

        for (let ring = 1; ring <= 4; ring++) {

            const points = [];

            for (let index = 0; index < categories.length; index++) {
                points.push(polar(cx, cy, radius * ring / 4,
                    startAngle + angleStep * index));
            }

            create('polygon', {
                points: points.map(function (point) {
                    return round(point.x) + ',' + round(point.y);
                }).join(' '),
                fill: 'none',
                class: 'tech-chart-grid'
            }, plot);
        }

        categories.forEach(function (category, index) {

            const angle = startAngle + angleStep * index;
            const edge = polar(cx, cy, radius, angle);

            create('line', {
                x1: round(cx),
                y1: round(cy),
                x2: round(edge.x),
                y2: round(edge.y),
                class: 'tech-chart-grid'
            }, plot);

            const label = polar(cx, cy, radius + 16, angle);

            text(plot, category, {
                x: round(label.x),
                y: round(label.y + 4),
                'text-anchor': Math.abs(Math.cos(angle * Math.PI / 180)) < 0.2
                    ? 'middle'
                    : (Math.cos(angle * Math.PI / 180) > 0 ? 'start' : 'end'),
                'font-size': 11,
                class: 'tech-chart-axis-label'
            });
        });

        allSeries.forEach(function (series) {

            const points = categories.map(function (category, index) {

                const value = num(series.data[index]) || 0;
                const ratio = clamp(value / Math.max(1e-9, maxValue), 0, 1);

                return polar(cx, cy, radius * ratio,
                    startAngle + angleStep * index);
            });

            create('polygon', {
                points: points.map(function (point) {
                    return round(point.x) + ',' + round(point.y);
                }).join(' '),
                fill: series.color,
                'fill-opacity': '.18',
                stroke: series.color,
                'stroke-width': 2
            }, plot);

            points.forEach(function (point, index) {

                const dot = create('circle', {
                    cx: round(point.x),
                    cy: round(point.y),
                    r: 3.5,
                    fill: series.color
                }, plot);

                bindTip(dot, ctx.tip, function () {

                    return tipHtml(categories[index], [{
                        name: series.name,
                        color: series.color,
                        value: ctx.format(num(series.data[index]) || 0,
                            ctx.opts.valueFormat)
                    }]);
                });
            });
        });

        seriesLegend(ctx, ctx.series);
    }

    function drawHeatmap(ctx, allSeries) {

        const categories = ctx.categories.length
            ? ctx.categories
            : resolveCategories(ctx, allSeries);

        if (categories.length === 0 || allSeries.length === 0) {
            showEmpty(ctx);
            seriesLegend(ctx, ctx.series);
            return;
        }

        let min = Infinity;
        let max = -Infinity;

        allSeries.forEach(function (series) {
            series.data.forEach(function (raw) {
                const value = num(raw);
                if (value === null) return;
                min = Math.min(min, value);
                max = Math.max(max, value);
            });
        });

        if (!Number.isFinite(min)) {
            showEmpty(ctx);
            return;
        }

        if (min === max) max = min + 1;

        const labelWidth = Math.min(
            ctx.width * 0.3,
            Math.max.apply(null, allSeries.map(function (series) {
                return estimate(series.name, 11);
            })) + 12);

        const pad = 14;
        const plotLeft = pad + labelWidth;
        const plotRight = ctx.width - pad;
        const plotTop = pad + 6;
        const plotBottom = ctx.height - pad - 24;

        if (plotRight - plotLeft < 30 || plotBottom - plotTop < 30) {
            showEmpty(ctx);
            return;
        }

        const gap = 3;
        const cellWidth = (plotRight - plotLeft -
            gap * (categories.length - 1)) / categories.length;

        const rowHeight = (plotBottom - plotTop -
            gap * (allSeries.length - 1)) / allSeries.length;

        const plot = create('g', { class: 'tech-chart-plot' }, ctx.svg);

        if (ctx.opts.animation !== false) {
            plot.classList.add('tech-chart-animate');
        }

        allSeries.forEach(function (series, row) {

            const y = plotTop + row * (rowHeight + gap);

            text(plot, series.name, {
                x: round(plotLeft - 8),
                y: round(y + rowHeight / 2 + 4),
                'text-anchor': textAnchor(ctx.dir, 'left'),
                'font-size': 11,
                class: 'tech-chart-axis-label'
            });

            series.data.forEach(function (raw, column) {

                const value = num(raw);

                if (value === null) return;

                const ratio = (value - min) / (max - min);
                const x = plotLeft + column * (cellWidth + gap);

                const cell = create('rect', {
                    x: round(x),
                    y: round(y),
                    width: Math.max(1, round(cellWidth)),
                    height: Math.max(1, round(rowHeight)),
                    rx: 3,
                    fill: series.color,
                    'fill-opacity': round(0.1 + ratio * 0.85)
                }, plot);

                bindTip(cell, ctx.tip, function () {

                    return tipHtml(categories[column] || '', [{
                        name: series.name,
                        color: series.color,
                        value: ctx.format(value, ctx.opts.valueFormat)
                    }]);
                });

                if (ctx.opts.dataLabels && cellWidth > 36) {

                    dataLabel(ctx, plot,
                        x + cellWidth / 2,
                        y + rowHeight / 2 + 4,
                        ctx.format(value, ctx.opts.valueFormat),
                        { color: ratio > 0.55 ? '#fff' : null });
                }
            });
        });

        categories.forEach(function (category, index) {

            if (index % Math.ceil(categories.length /
                Math.max(1, Math.floor((plotRight - plotLeft) / 60))) !== 0) {
                return;
            }

            text(plot, category, {
                x: round(plotLeft + index * (cellWidth + gap) +
                    cellWidth / 2),
                y: round(plotBottom + 18),
                'text-anchor': 'middle',
                'font-size': 11,
                class: 'tech-chart-axis-label'
            });
        });

        seriesLegend(ctx, ctx.series);
    }

    function drawFunnel(ctx, allSeries) {

        const series = allSeries[0];

        if (!series) {
            showEmpty(ctx);
            return;
        }

        const hidden = ctx.instance.hiddenSlices;
        const categories = ctx.categories.length
            ? ctx.categories
            : resolveCategories(ctx, allSeries);

        const allItems = series.data.map(function (raw, index) {
            return {
                key: index,
                label: categories[index] || String(index + 1),
                value: Math.max(0, num(raw) || 0),
                color: ctx.palette[index % ctx.palette.length]
            };
        });

        const items = allItems.filter(function (item) {
            return !hidden.has(item.key);
        });

        if (items.length === 0) {
            showEmpty(ctx);
            sliceLegend(ctx, allItems);
            return;
        }

        const maxValue = Math.max.apply(null,
            items.map(function (item) { return item.value; }));

        if (maxValue <= 0) {
            showEmpty(ctx);
            return;
        }

        const pad = 20;
        const plotTop = pad;
        const plotBottom = ctx.height - pad;
        const plotLeft = pad;
        const plotRight = ctx.width - pad;

        const gap = 6;
        const rowHeight = (plotBottom - plotTop -
            gap * (items.length - 1)) / items.length;

        const plot = create('g', { class: 'tech-chart-plot' }, ctx.svg);

        if (ctx.opts.animation !== false) {
            plot.classList.add('tech-chart-animate');
        }

        items.forEach(function (item, index) {

            const width = Math.max(30,
                (item.value / maxValue) * (plotRight - plotLeft));
            const x = plotLeft + (plotRight - plotLeft - width) / 2;
            const y = plotTop + index * (rowHeight + gap);

            const shape = create('rect', {
                x: round(x),
                y: round(y),
                width: round(width),
                height: Math.max(2, round(rowHeight)),
                rx: 4,
                fill: item.color
            }, plot);

            bindTip(shape, ctx.tip, function () {

                return tipHtml(item.label, [{
                    name: series.name,
                    color: item.color,
                    value: ctx.format(item.value, ctx.opts.valueFormat)
                }]);
            });

            text(plot, item.label, {
                x: round(x + 10),
                y: round(y + rowHeight / 2 + 4),
                'text-anchor': textAnchor(ctx.dir, 'right'),
                'font-size': 11,
                class: 'tech-chart-oncolor'
            });

            text(plot, ctx.format(item.value, ctx.opts.valueFormat), {
                x: round(x + width - 10),
                y: round(y + rowHeight / 2 + 4),
                'text-anchor': textAnchor(ctx.dir, 'left'),
                'font-size': 11,
                class: 'tech-chart-oncolor'
            });
        });

        sliceLegend(ctx, allItems.map(function (item) {
            return {
                key: item.key,
                label: item.label,
                color: item.color
            };
        }));
    }

    function drawTreemap(ctx, allSeries) {

        const series = allSeries[0];

        if (!series) {
            showEmpty(ctx);
            return;
        }

        const hidden = ctx.instance.hiddenSlices;
        const categories = ctx.categories.length
            ? ctx.categories
            : resolveCategories(ctx, allSeries);

        const allItems = series.data.map(function (raw, index) {
            return {
                key: index,
                label: categories[index] || String(index + 1),
                value: Math.max(0, num(raw) || 0),
                color: ctx.palette[index % ctx.palette.length]
            };
        }).filter(function (item) {
            return item.value > 0;
        });

        const items = allItems.filter(function (item) {
            return !hidden.has(item.key);
        });

        if (items.length === 0) {
            showEmpty(ctx);
            sliceLegend(ctx, allItems);
            return;
        }

        const pad = 8;
        const box = {
            x: pad,
            y: pad,
            w: ctx.width - pad * 2,
            h: ctx.height - pad * 2
        };

        const rects = squarify(items, box);

        const plot = create('g', { class: 'tech-chart-plot' }, ctx.svg);

        if (ctx.opts.animation !== false) {
            plot.classList.add('tech-chart-animate');
        }

        rects.forEach(function (item) {

            const rect = create('rect', {
                x: round(item.x + 1.5),
                y: round(item.y + 1.5),
                width: Math.max(1, round(item.w - 3)),
                height: Math.max(1, round(item.h - 3)),
                rx: 4,
                fill: item.color
            }, plot);

            bindTip(rect, ctx.tip, function () {

                return tipHtml(item.label, [{
                    name: 'مقدار',
                    color: item.color,
                    value: ctx.format(item.value, ctx.opts.valueFormat)
                }]);
            });

            if (item.w > 54 && item.h > 28) {

                text(plot, item.label, {
                    x: round(item.x + 8),
                    y: round(item.y + 18),
                    'font-size': 11,
                    class: 'tech-chart-oncolor',
                    'font-weight': 600
                });

                text(plot, ctx.format(item.value, ctx.opts.valueFormat), {
                    x: round(item.x + 8),
                    y: round(item.y + 33),
                    'font-size': 10,
                    class: 'tech-chart-oncolor',
                    'fill-opacity': '.85'
                });
            }
        });

        sliceLegend(ctx, allItems.map(function (item) {
            return {
                key: item.key,
                label: item.label,
                color: item.color
            };
        }));
    }

    function worstRatio(row, side) {

        const total = sumOf(row.map(function (item) { return item.area; }));
        const max = maxOf(row.map(function (item) { return item.area; }));
        const min = Math.min.apply(null,
            row.map(function (item) { return item.area; }));

        if (total <= 0 || side <= 0) return Infinity;

        return Math.max(
            side * side * max / (total * total),
            total * total / (side * side * min));
    }

    function layoutRow(row, box, rects) {

        const total = sumOf(row.map(function (item) { return item.area; }));

        if (total <= 0) return;

        if (box.w >= box.h) {

            const height = Math.min(box.h, total / box.w);
            let x = box.x;

            row.forEach(function (item) {
                const width = item.area / Math.max(1e-9, height);
                rects.push({
                    key: item.key,
                    label: item.label,
                    value: item.value,
                    color: item.color,
                    x: x,
                    y: box.y,
                    w: width,
                    h: height
                });
                x += width;
            });

            box.y += height;
            box.h -= height;
        }
        else {

            const width = Math.min(box.w, total / box.h);
            let y = box.y;

            row.forEach(function (item) {
                const height = item.area / Math.max(1e-9, width);
                rects.push({
                    key: item.key,
                    label: item.label,
                    value: item.value,
                    color: item.color,
                    x: box.x,
                    y: y,
                    w: width,
                    h: height
                });
                y += height;
            });

            box.x += width;
            box.w -= width;
        }
    }

    function squarify(items, box) {

        const total = sumOf(items.map(function (item) { return item.value; }));
        const scale = box.w * box.h / Math.max(1e-9, total);

        const nodes = items.map(function (item) {
            return Object.assign({}, item, { area: item.value * scale });
        });

        const rects = [];
        let row = [];

        while (nodes.length > 0) {

            const side = Math.min(box.w, box.h);
            const candidate = row.concat([nodes[0]]);

            if (row.length === 0 ||
                worstRatio(candidate, side) <= worstRatio(row, side)) {
                row.push(nodes.shift());
            }
            else {
                layoutRow(row, box, rects);
                row = [];
            }
        }

        if (row.length > 0) layoutRow(row, box, rects);

        return rects;
    }

    function drawBullet(ctx, allSeries) {

        const series = allSeries[0];
        const targetSeries = allSeries[1];

        if (!series) {
            showEmpty(ctx);
            return;
        }

        const categories = ctx.categories.length
            ? ctx.categories
            : resolveCategories(ctx, allSeries);

        const actual = series.data.map(num);

        const targets = series.targets.length > 0
            ? series.targets.map(num)
            : targetSeries
                ? targetSeries.data.map(num)
                : [];

        const xAxis = ctx.opts.xAxis || {};
        const maxValue = (xAxis.max !== null &&
            xAxis.max !== undefined)
            ? Number(xAxis.max)
            : Math.max(1,
                maxOf(actual.concat(targets)) * 1.08);

        const minValue = (xAxis.min !== null &&
            xAxis.min !== undefined) ? Number(xAxis.min) : 0;

        const labelWidth = Math.min(
            ctx.width * 0.3,
            Math.max.apply(null, categories.map(function (category) {
                return estimate(category, 11);
            })) + 14);

        const pad = 16;
        const plotLeft = pad + labelWidth;
        const plotRight = ctx.width - pad - 34;
        const plotTop = pad + 6;
        const plotBottom = ctx.height - pad - 22;

        if (plotRight - plotLeft < 40) {
            showEmpty(ctx);
            return;
        }

        const x = function (value) {
            return plotLeft +
                ((value - minValue) / Math.max(1e-9, maxValue - minValue)) *
                (plotRight - plotLeft);
        };

        const slot = (plotBottom - plotTop) / Math.max(1, categories.length);
        const barHeight = Math.min(18, slot * 0.42);

        const plot = create('g', { class: 'tech-chart-plot' }, ctx.svg);

        if (ctx.opts.animation !== false) {
            plot.classList.add('tech-chart-animate');
        }

        categories.forEach(function (category, index) {

            const centerY = plotTop + slot * (index + 0.5);

            create('rect', {
                x: round(plotLeft),
                y: round(centerY - barHeight / 2),
                width: round(plotRight - plotLeft),
                height: round(barHeight),
                rx: 4,
                fill: 'var(--tech-border)',
                'fill-opacity': '.55'
            }, plot);

            const value = actual[index];

            if (value !== null && Number.isFinite(value)) {

                const bar = create('rect', {
                    x: round(Math.min(x(minValue), x(value))),
                    y: round(centerY - barHeight / 2),
                    width: Math.max(1, round(Math.abs(x(value) - x(minValue)))),
                    height: round(barHeight),
                    rx: 4,
                    fill: series.color
                }, plot);

                bindTip(bar, ctx.tip, function () {

                    const rows = [{
                        name: series.name,
                        color: series.color,
                        value: ctx.format(value, ctx.opts.valueFormat)
                    }];

                    const target = targets[index];

                    if (target !== null && target !== undefined) {
                        rows.push({
                            name: 'هدف',
                            value: ctx.format(target, ctx.opts.valueFormat)
                        });
                    }

                    return tipHtml(category, rows);
                });
            }

            const target = targets[index];

            if (target !== null && target !== undefined) {

                create('line', {
                    x1: round(x(target)),
                    y1: round(centerY - barHeight),
                    x2: round(x(target)),
                    y2: round(centerY + barHeight),
                    stroke: 'var(--tech-text)',
                    'stroke-width': 2,
                    'stroke-linecap': 'round'
                }, plot);
            }

            text(plot, category, {
                x: round(plotLeft - 8),
                y: round(centerY + 4),
                'text-anchor': textAnchor(ctx.dir, 'left'),
                'font-size': 11,
                class: 'tech-chart-axis-label'
            });

            text(plot, value === null
                ? '—'
                : ctx.format(value, ctx.opts.valueFormat), {
                x: round(plotRight + 8),
                y: round(centerY + 4),
                'text-anchor': textAnchor(ctx.dir, 'right'),
                'font-size': 11,
                class: 'tech-chart-axis-label'
            });
        });
    }

    function drawSparkline(ctx) {

        const series = visibleSeries(ctx)[0];

        if (!series || series.data.length === 0) {
            showEmpty(ctx);
            return;
        }

        const values = series.data.map(num);
        const maxValue = maxOf(values);
        const minValue = Math.min.apply(null,
            values.filter(function (value) { return value !== null; }));

        const min = Number.isFinite(minValue) ? minValue : 0;
        const max = Number.isFinite(maxValue) ? maxValue : 1;
        const span = max === min ? 1 : max - min;

        const pad = 5;
        const plotLeft = pad;
        const plotRight = ctx.width - pad;
        const plotTop = pad;
        const plotBottom = ctx.height - pad;

        const step = (plotRight - plotLeft) /
            Math.max(1, values.length - 1);

        const y = function (value) {
            return plotBottom -
                ((value - min) / span) * (plotBottom - plotTop);
        };

        const plot = create('g', { class: 'tech-chart-plot' }, ctx.svg);

        if (ctx.opts.animation !== false) {
            plot.classList.add('tech-chart-animate');
        }

        if (ctx.type === 'sparkbar') {

            const width = Math.max(1,
                (plotRight - plotLeft) / values.length * 0.68);

            values.forEach(function (value, index) {

                if (value === null) return;

                const x = plotLeft + (plotRight - plotLeft) /
                    values.length * (index + 0.5);

                const rect = create('rect', {
                    x: round(x - width / 2),
                    y: round(y(value)),
                    width: round(width),
                    height: Math.max(1, round(plotBottom - y(value))),
                    rx: 2,
                    fill: series.color
                }, plot);

                bindTip(rect, ctx.tip, function () {
                    return tipHtml(String(index + 1), [{
                        name: series.name,
                        color: series.color,
                        value: ctx.format(value, ctx.opts.valueFormat)
                    }]);
                });
            });

            return;
        }

        const points = values.map(function (value, index) {

            if (value === null) return null;

            return {
                x: plotLeft + step * index,
                y: y(value),
                value: value,
                index: index
            };
        });

        const segments = findSegments(points);
        const mode = ctx.opts.smooth === false ? 'line' : 'smooth';

        segments.forEach(function (segment, segmentIndex) {

            if (ctx.opts.area === true) {

                create('path', {
                    d: areaPath(segment, mode, plotBottom),
                    fill: addGradient(ctx, series.color, 'spark-' + segmentIndex),
                    stroke: 'none'
                }, plot);
            }

            create('path', {
                d: linePath(segment, mode),
                fill: 'none',
                stroke: series.color,
                'stroke-width': 2,
                'stroke-linecap': 'round',
                'stroke-linejoin': 'round',
                'stroke-dasharray': series.dash ? '5 4' : null
            }, plot);
        });

        const last = points.filter(function (point) { return point !== null; })
            .slice(-1)[0];

        if (last) {

            create('circle', {
                cx: round(last.x),
                cy: round(last.y),
                r: 3,
                fill: series.color
            }, plot);
        }

        points.forEach(function (point) {

            if (point === null) return;

            const hit = create('circle', {
                cx: round(point.x),
                cy: round(point.y),
                r: 8,
                fill: 'transparent'
            }, plot);

            bindTip(hit, ctx.tip, function () {
                return tipHtml(String(point.index + 1), [{
                    name: series.name,
                    color: series.color,
                    value: ctx.format(point.value, ctx.opts.valueFormat)
                }]);
            });
        });
    }

    function defaultHeight(type) {

        if (type === 'sparkline' || type === 'sparkbar') return 64;
        if (type === 'gauge') return 260;
        if (type === 'radialbar') return 280;
        if (type === 'pie' || type === 'donut' || type === 'polar' ||
            type === 'radar' || type === 'funnel' || type === 'treemap') {
            return 320;
        }

        return 340;
    }

    function normalizeType(value) {

        const type = String(value || 'column').trim().toLowerCase();

        if (type === 'area-spline') return 'areaspline';
        if (type === 'horizontalbar' || type === 'hbar') return 'bar';
        if (type === 'radial') return 'radialbar';

        return type;
    }

    function render(instance) {

        const root = instance.element;
        const canvas = root.querySelector('.tech-chart-canvas');

        if (!canvas) return;

        const options = instance.options || {};
        const type = normalizeType(options.type);

        const height = clamp(
            Number(options.height) || defaultHeight(type),
            type === 'sparkline' || type === 'sparkbar' ? 40 : 120,
            1600);

        canvas.style.height = height + 'px';
        canvas.innerHTML = '';

        removeLegend(root);

        const width = Math.max(220, canvas.clientWidth || 640);

        const tip = document.createElement('div');

        tip.className = 'tech-chart-tooltip';
        canvas.appendChild(tip);

        const svgRoot = create('svg', {
            viewBox: '0 0 ' + width + ' ' + height,
            width: '100%',
            height: '100%',
            'aria-hidden': 'true',
            focusable: 'false'
        }, canvas);

        const rtl = options.rtl === true
            ? true
            : options.rtl === false
                ? false
                : getComputedStyle(root).direction === 'rtl';

        svgRoot.style.direction = rtl ? 'rtl' : 'ltr';

        const locale = resolveLocale(root, options);

        const palette = resolvePalette(root, options.colors);

        const categories = Array.isArray(options.categories)
            ? options.categories.map(function (value) {
                return value === null || value === undefined
                    ? ''
                    : String(value);
            })
            : [];

        const series = (Array.isArray(options.series) ? options.series : [])
            .map(function (item, index) {

                const seriesType = normalizeType(item.type || type);

                return {
                    index: index,
                    name: item.name || ('سری ' + (index + 1)),
                    type: seriesType,
                    color: item.color || palette[index % palette.length],
                    data: Array.isArray(item.data)
                        ? item.data.map(function (value, dataIndex) {
                            return dataIndex < item.data.length
                                ? (value === null || value === ''
                                    ? null
                                    : value)
                                : null;
                        })
                        : [],
                    points: Array.isArray(item.points)
                        ? item.points
                        : [],
                    candles: Array.isArray(item.candles)
                        ? item.candles
                        : [],
                    targets: Array.isArray(item.targets)
                        ? item.targets
                        : [],
                    dash: item.dash === true,
                    opacity: item.opacity === null ||
                        item.opacity === undefined
                        ? null
                        : Number(item.opacity),
                    stack: item.stack || null
                };
            });

        series.forEach(function (item) {
            if (item.hidden) instance.hiddenSeries.add(item.index);
        });

        const ctx = {
            root: root,
            canvas: canvas,
            svg: svgRoot,
            tip: tip,
            opts: options,
            type: type,
            width: width,
            height: height,
            dir: rtl,
            locale: locale,
            palette: palette,
            categories: categories,
            series: series,
            instance: instance,
            uid: instance.uid,
            format: function (value, format) {
                return formatValue(value, format || options.valueFormat, locale);
            }
        };

        const visible = visibleSeries(ctx);

        if (ctx.series.length === 0) {
            showEmpty(ctx);
            return;
        }

        if (type === 'pie' || type === 'donut' || type === 'polar') {
            drawPie(ctx, visible);
        }
        else if (type === 'gauge') {
            drawGauge(ctx, visible);
        }
        else if (type === 'radialbar') {
            drawRadialBar(ctx, visible);
        }
        else if (type === 'radar') {
            drawRadar(ctx, visible);
        }
        else if (type === 'heatmap') {
            drawHeatmap(ctx, visible);
        }
        else if (type === 'funnel') {
            drawFunnel(ctx, visible);
        }
        else if (type === 'treemap') {
            drawTreemap(ctx, visible);
        }
        else if (type === 'bullet') {
            drawBullet(ctx, visible);
        }
        else if (type === 'sparkline' || type === 'sparkbar') {
            drawSparkline(ctx);
        }
        else if (type === 'scatter' || type === 'bubble') {
            drawScatter(ctx, visible);
        }
        else if (type === 'candlestick') {
            drawCandles(ctx, visible);
        }
        else {
            drawCartesian(ctx);
        }

        const label = options.ariaLabel ||
            root.getAttribute('aria-label') ||
            'نمودار';

        canvas.setAttribute('aria-label', label);
    }

    function parseOptions(root) {

        const raw = root.getAttribute('data-chart-options');

        if (!raw) return {};

        try {
            return JSON.parse(raw);
        }
        catch (error) {
            console.warn('TechUI chart: invalid options JSON.', error);
            return {};
        }
    }

    function initChart(root) {

        if (!root || root._techChart) return;

        root._techChart = true;

        const canvas = root.querySelector('.tech-chart-canvas');

        if (!canvas) return;

        const instance = {
            element: root,
            uid: Math.random().toString(36).slice(2, 9),
            options: parseOptions(root),
            hiddenSeries: new Set(),
            hiddenSlices: new Set(),
            render: function () {
                render(instance);
            },
            refresh: function () {
                render(instance);
            },
            update: function (next) {
                instance.options = Object.assign({}, instance.options,
                    next || {});
                render(instance);
            },
            destroy: function () {

                if (instance.observer) instance.observer.disconnect();

                if (instance.onWindowResize) {
                    window.removeEventListener('resize',
                        instance.onWindowResize);
                }

                instance.element._techChart = false;
                instance.element._techChartInstance = null;

                if (instance.element.id) {
                    Tech.Chart.instances.delete(instance.element.id);
                }

                canvas.innerHTML = '';
                removeLegend(instance.element);
            }
        };

        root._techChartInstance = instance;

        if (root.id) {
            Tech.Chart.instances.set(root.id, instance);
        }

        render(instance);

        if (window.ResizeObserver) {

            let frame = null;
            let lastWidth = canvas.clientWidth;

            instance.observer = new ResizeObserver(function () {

                const width = canvas.clientWidth;

                if (!width || Math.abs(width - lastWidth) < 2) return;

                lastWidth = width;

                if (frame) cancelAnimationFrame(frame);

                frame = requestAnimationFrame(function () {
                    frame = null;
                    render(instance);
                });
            });

            instance.observer.observe(canvas);
        }

        instance.onWindowResize = function () {
            render(instance);
        };

        window.addEventListener('resize', instance.onWindowResize);
    }

    Tech.Chart.get = function (id) {
        return Tech.Chart.instances.get(id) || null;
    };

    Tech.Chart.render = function (target) {

        const element = typeof target === 'string'
            ? document.querySelector(target)
            : target;

        if (!element) return null;

        initChart(element);

        return element._techChartInstance || null;
    };

    Tech.Chart.update = function (id, options) {

        const instance = Tech.Chart.get(id);

        if (!instance) return null;

        instance.update(options);

        return instance;
    };

    document.addEventListener('tech:theme-changed', function () {

        Tech.Chart.instances.forEach(function (instance) {
            instance.render();
        });
    });

    Tech.register('chart', initChart);
})();


// ------------------------------------------------------------
// Chat
// ------------------------------------------------------------

(function () {

    Tech.Chat = Tech.Chat || {};
    Tech.Chat.instances = Tech.Chat.instances || new Map();

    const roles = ['user', 'assistant', 'agent', 'system'];

    function field(obj, name) {

        if (!obj) return undefined;

        if (obj[name] !== undefined) return obj[name];

        const pascal = name.charAt(0).toUpperCase() + name.slice(1);

        return obj[pascal];
    }

    function normalizeRole(role) {

        const value = String(role || '').toLowerCase();

        if (value === 'error') return 'error';

        return roles.indexOf(value) >= 0 ? value : 'assistant';
    }

    function nowTime() {

        try {

            return new Intl.DateTimeFormat('fa-IR', {
                hour: '2-digit',
                minute: '2-digit'
            }).format(new Date());
        }
        catch {
            return '';
        }
    }

    function newConversationId() {

        if (window.crypto && typeof window.crypto.randomUUID === 'function') {
            return window.crypto.randomUUID();
        }

        return 'chat-' + Date.now().toString(36) + '-' +
            Math.random().toString(36).slice(2, 8);
    }

    function parseOptions(el) {

        try {
            return JSON.parse(el.dataset.techChat || '{}') || {};
        }
        catch {
            return {};
        }
    }

    async function postJson(url, body, element) {

        const headers = {
            'Accept': 'application/json',
            'Content-Type': 'application/json; charset=UTF-8'
        };

        const token = Tech.getAntiforgeryToken(element);

        if (token) headers['RequestVerificationToken'] = token;

        const response = await fetch(url, {
            method: 'POST',
            headers: headers,
            credentials: 'same-origin',
            body: JSON.stringify(body)
        });

        if (!response.ok) {
            throw new Error(
                `HTTP ${response.status} ${response.statusText}`.trim());
        }

        return response.json();
    }

    function initChat(el) {

        if (el._techChatInstance) return el._techChatInstance;

        const options = parseOptions(el);

        options.mode = options.mode === 'support' ? 'support' : 'assistant';
        options.pollInterval = Math.max(2, parseInt(options.pollInterval, 10) || 4);
        options.maxLength = parseInt(options.maxLength, 10) || 1000;

        const log = el.querySelector('.tech-chat-log');
        const form = el.querySelector('.tech-chat-composer');
        const input = el.querySelector('.tech-chat-input');
        const sendButton = el.querySelector('.tech-chat-send');
        const clearButton = el.querySelector('[data-chat-action="clear"]');

        if (!log || !form || !input) return null;

        const state = {
            messages: [],
            busy: false,
            typing: false,
            lastId: 0,
            conversationId: newConversationId(),
            timer: null,
            pollInFlight: false,
            generation: 0,
            destroyed: false
        };

        const key = el.id || el;

        if (input.getAttribute('maxlength')) {
            input.setAttribute(
                'maxlength', String(options.maxLength));
        }

        function autosize() {

            input.style.height = 'auto';
            input.style.height = Math.min(input.scrollHeight, 112) + 'px';
        }

        function syncComposer() {

            if (sendButton) sendButton.disabled = state.busy;
            form.setAttribute('aria-busy', state.busy ? 'true' : 'false');
            el.classList.toggle('tech-chat-busy', state.busy);
        }

        function messageHtml(message) {

            const role = message.role === 'error'
                ? 'error'
                : normalizeRole(message.role);

            const meta = [];

            if (role !== 'user' && message.sender) {
                meta.push(message.sender);
            }

            if (message.time) meta.push(message.time);

            return '<div class="tech-chat-message tech-chat-message-' + role + '">' +
                '<div class="tech-chat-body">' +
                '<div class="tech-chat-bubble">' +
                Tech.escapeHtml(message.content) +
                '</div>' +
                (meta.length > 0
                    ? '<span class="tech-chat-meta">' +
                        Tech.escapeHtml(meta.join(' · ')) +
                        '</span>'
                    : '') +
                '</div></div>';
        }

        function typingHtml() {

            const role = options.mode === 'support' ? 'agent' : 'assistant';

            return '<div class="tech-chat-message tech-chat-message-' + role + '">' +
                '<div class="tech-chat-body">' +
                '<div class="tech-chat-bubble tech-chat-typing" aria-label="در حال نوشتن">' +
                '<span></span><span></span><span></span>' +
                '</div></div></div>';
        }

        function render() {

            const nearBottom = log.scrollHeight - log.scrollTop - log.clientHeight < 48;

            let html = '';

            if (state.messages.length === 0) {

                if (options.welcome) {

                    html += '<div class="tech-chat-welcome">' +
                        Tech.escapeHtml(options.welcome) +
                        '</div>';
                }

                if (Array.isArray(options.suggestions) &&
                    options.suggestions.length > 0) {

                    html += '<div class="tech-chat-suggestions">' +
                        options.suggestions.map(function (suggestion) {
                            return '<button type="button" class="tech-chat-chip" ' +
                                `data-text="${Tech.escapeHtml(suggestion)}">` +
                                Tech.escapeHtml(suggestion) +
                                '</button>';
                        }).join('') +
                        '</div>';
                }
            }

            state.messages.forEach(function (message) {
                html += messageHtml(message);
            });

            if (state.typing) html += typingHtml();

            log.innerHTML = html;

            if (nearBottom || state.busy) {
                log.scrollTop = log.scrollHeight;
            }
        }

        function append(message, notify) {

            const normalized = {
                role: normalizeRole(message.role),
                content: String(message.content == null ? '' : message.content),
                sender: message.sender || null,
                time: message.time || nowTime()
            };

            state.messages.push(normalized);
            render();

            if (notify !== false) {
                Tech.trigger(el, 'Receive', normalized);
            }

            return normalized;
        }

        function readServerMessages(data, includeUser) {

            const list = field(data, 'messages');

            if (!Array.isArray(list)) return;

            list.forEach(function (item) {

                const role = normalizeRole(field(item, 'role'));

                if (role === 'user' && !includeUser) return;

                const id = Number(field(item, 'id'));

                if (id && id > state.lastId) state.lastId = id;

                append({
                    role: role,
                    content: field(item, 'content') || '',
                    sender: field(item, 'sender') || null,
                    time: field(item, 'time') || null
                });
            });
        }

        function poll() {

            if (state.destroyed || state.pollInFlight ||
                !options.pollUrl || document.hidden) {
                return;
            }

            state.pollInFlight = true;

            const generation = state.generation;

            const params = new URLSearchParams({
                conversation: state.conversationId,
                after: String(state.lastId)
            });

            Tech.requestJson(options.pollUrl, { params: params })
                .then(function (data) {

                    if (state.destroyed || generation !== state.generation) return;

                    readServerMessages(data, false);

                    const typing = field(data, 'typing');

                    if (typing !== undefined && !!typing !== state.typing) {
                        state.typing = !!typing;
                        render();
                    }
                })
                .catch(function (err) {
                    console.warn('TechUI Chat polling failed:', err);
                })
                .finally(function () {
                    state.pollInFlight = false;
                });
        }

        function startPolling() {

            if (!options.pollUrl || state.timer || state.destroyed) return;

            state.timer = window.setInterval(
                poll, options.pollInterval * 1000);

            poll();
        }

        function stopPolling() {

            if (!state.timer) return;

            window.clearInterval(state.timer);
            state.timer = null;
        }

        async function send(raw) {

            const text = String(raw == null ? '' : raw).trim();

            if (!text || state.busy || state.destroyed) return;

            const generation = state.generation;

            const history = state.messages
                .filter(function (message) {
                    return message.role === 'user' ||
                        message.role === 'assistant' ||
                        message.role === 'agent';
                })
                .slice(-12)
                .map(function (message) {
                    return {
                        role: message.role,
                        content: message.content
                    };
                });

            input.value = '';
            autosize();

            append({ role: 'user', content: text }, false);

            Tech.trigger(el, 'Send', { message: text });

            state.busy = true;
            state.typing = options.mode !== 'support';
            render();
            syncComposer();

            try {

                const data = await postJson(options.url, {
                    message: text,
                    conversationId: state.conversationId,
                    sender: options.sender || null,
                    history: history
                }, el);

                if (state.destroyed || generation !== state.generation) return;

                const messageId = Number(field(data, 'messageId'));

                if (messageId && messageId > state.lastId) {
                    state.lastId = messageId;
                }

                const typing = field(data, 'typing');

                state.typing = typing === undefined ? false : !!typing;

                const reply = field(data, 'reply');

                if (typeof reply === 'string' && reply.trim()) {

                    append({
                        role: 'assistant',
                        content: reply,
                        sender: field(data, 'sender') || null
                    });
                }

                readServerMessages(data, false);

                if (options.mode === 'support') startPolling();
            }
            catch (err) {

                if (state.destroyed || generation !== state.generation) return;

                console.error('TechUI Chat error:', err);

                state.typing = false;

                append({
                    role: 'error',
                    content: 'ارتباط با سرور برقرار نشد. دوباره تلاش کنید.'
                }, false);

                Tech.trigger(el, 'Error', { error: err });
            }
            finally {

                if (!state.destroyed && generation === state.generation) {

                    state.busy = false;
                    render();
                    syncComposer();
                }
            }
        }

        function clear() {

            state.generation++;
            state.messages = [];
            state.typing = false;
            state.busy = false;
            state.lastId = 0;
            state.conversationId = newConversationId();

            render();
            syncComposer();

            Tech.trigger(el, 'Clear', {});
        }

        form.addEventListener('submit', function (ev) {

            ev.preventDefault();

            send(input.value);
        });

        input.addEventListener('input', autosize);

        input.addEventListener('keydown', function (ev) {

            if (ev.key === 'Enter' && !ev.shiftKey && !ev.isComposing) {

                ev.preventDefault();

                send(input.value);
            }
        });

        log.addEventListener('click', function (ev) {

            const chip = ev.target.closest('.tech-chat-chip');

            if (chip) send(chip.dataset.text || '');
        });

        if (clearButton) {
            clearButton.addEventListener('click', clear);
        }

        document.addEventListener('visibilitychange', function () {

            if (!document.hidden && state.timer) poll();
        });

        const instance = {

            id: el.id || null,

            element: el,

            getMessages: function () {
                return state.messages.slice();
            },

            send: send,

            receive: function (message) {

                if (typeof message === 'string') {
                    return append({ role: 'agent', content: message });
                }

                return append(message || {});
            },

            typing: function (show) {

                state.typing = !!show;
                render();
            },

            clear: clear,

            startPolling: startPolling,

            stopPolling: stopPolling,

            focus: function () {
                input.focus();
            },

            isBusy: function () {
                return state.busy;
            },

            destroy: function () {

                state.destroyed = true;
                stopPolling();

                Tech.Chat.instances.delete(key);

                el._techChatInstance = null;
                el.removeAttribute('data-tech-chat-bound');
            }
        };

        el._techChatInstance = instance;

        Tech.Chat.instances.set(key, instance);

        render();
        syncComposer();
        autosize();

        return instance;
    }

    Tech.Chat.get = function (target) {

        if (target && target.nodeType === 1) {
            return target._techChatInstance || null;
        }

        const el = document.getElementById(target);

        return el ? el._techChatInstance || null : null;
    };

    Tech.register('chat', initChat);
})();


// ------------------------------------------------------------
// Map
// ------------------------------------------------------------

(function () {

    const TILE_SIZE = 256;
    const MAX_LAT = 85.05112878;
    const SVG_NS = 'http://www.w3.org/2000/svg';
    const TONES = ['primary', 'success', 'info', 'warning', 'danger', 'neutral'];
    const SCALE_STEPS = [
        1, 2, 5, 10, 20, 50, 100, 200, 500,
        1000, 2000, 5000, 10000, 20000, 50000,
        100000, 200000, 500000, 1000000, 2000000, 5000000
    ];

    Tech.Map = Tech.Map || {};
    Tech.Map.instances = Tech.Map.instances || new Map();

    function parseMapOptions(root) {

        try {
            return JSON.parse(root.dataset.mapOptions || '{}') || {};
        }
        catch (err) {

            console.error('TechUI map options error:', err);

            return {};
        }
    }

    function number(value, fallback) {

        const parsed = Number(value);

        return isFinite(parsed) ? parsed : fallback;
    }

    function clamp(value, min, max) {

        return Math.max(min, Math.min(max, value));
    }

    function clampLat(lat) {

        return clamp(number(lat, 0), -MAX_LAT, MAX_LAT);
    }

    function wrapLng(lng) {

        const value = number(lng, 0);

        return ((value + 180) % 360 + 360) % 360 - 180;
    }

    function shortestLngDelta(from, to) {

        let delta = wrapLng(to) - wrapLng(from);

        if (delta > 180) delta -= 360;
        if (delta < -180) delta += 360;

        return delta;
    }

    function project(lat, lng, zoom) {

        const size = TILE_SIZE * Math.pow(2, zoom);
        const sin = Math.sin(clampLat(lat) * Math.PI / 180);

        return {
            x: (wrapLng(lng) + 180) / 360 * size,
            y: (0.5 - Math.log((1 + sin) / (1 - sin)) / (4 * Math.PI)) * size
        };
    }

    function unproject(x, y, zoom) {

        const size = TILE_SIZE * Math.pow(2, zoom);
        const n = Math.PI - 2 * Math.PI * y / size;

        return {
            lat: 180 / Math.PI * Math.atan(
                0.5 * (Math.exp(n) - Math.exp(-n))),
            lng: wrapLng(x / size * 360 - 180)
        };
    }

    function metersPerPixel(lat, zoom) {

        return 156543.03392 *
            Math.cos(clampLat(lat) * Math.PI / 180) /
            Math.pow(2, zoom);
    }

    function escapeHtml(value) {

        return String(value === null || value === undefined ? '' : value)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
    }

    function screenPoint(instance, lat, lng) {

        const point = project(lat, lng, instance.zoom);
        const center = project(
            instance.center.lat, instance.center.lng, instance.zoom);

        return {
            x: point.x - center.x + instance.width / 2 + instance.viewOffset.x,
            y: point.y - center.y + instance.height / 2 + instance.viewOffset.y
        };
    }

    function screenToLatLng(instance, x, y) {

        const center = project(
            instance.center.lat, instance.center.lng, instance.zoom);

        return unproject(
            center.x + x - instance.width / 2 - instance.viewOffset.x,
            center.y + y - instance.height / 2 - instance.viewOffset.y,
            instance.zoom);
    }

    function localPoint(instance, event) {

        const rect = instance.viewport.getBoundingClientRect();

        return {
            x: event.clientX - rect.left,
            y: event.clientY - rect.top
        };
    }

    function viewState(instance) {

        const northWest = screenToLatLng(instance, 0, 0);
        const southEast = screenToLatLng(
            instance, instance.width, instance.height);

        return {
            center: {
                lat: instance.center.lat,
                lng: instance.center.lng
            },
            zoom: instance.zoom,
            bounds: {
                north: northWest.lat,
                west: northWest.lng,
                south: southEast.lat,
                east: southEast.lng
            }
        };
    }

    function tileUrl(instance, x, y, zoom) {

        const layer = instance.layers[instance.activeIndex] ||
            instance.layers[0];

        const subdomains = layer.subdomains &&
            layer.subdomains.length
            ? layer.subdomains
            : (Array.isArray(instance.options.subdomains)
                ? instance.options.subdomains
                : []);

        const subdomain = subdomains.length
            ? subdomains[Math.abs(x + y) % subdomains.length]
            : '';

        return String(layer.url)
            .replace(/\{s\}/g, subdomain)
            .replace(/\{z\}/g, zoom)
            .replace(/\{x\}/g, x)
            .replace(/\{y\}/g, y)
            .replace(/\{-y\}/g, Math.pow(2, zoom) - 1 - y);
    }

    function createButton(icon, label, extraClass) {

        const button = document.createElement('button');

        button.type = 'button';
        button.className = 'tech-map-btn' +
            (extraClass ? ' ' + extraClass : '');
        button.setAttribute('aria-label', label);
        button.title = label;

        const iconElement = document.createElement('i');

        iconElement.className = 'bi bi-' + icon;
        iconElement.setAttribute('aria-hidden', 'true');

        button.appendChild(iconElement);

        return button;
    }

    function createControlGroup() {

        const group = document.createElement('div');

        group.className = 'tech-map-control tech-map-control-group';

        return group;
    }

    // --------------------------------------------------------
    // Events
    // --------------------------------------------------------

    function emit(instance, name, payload) {

        const events = instance.options.events || {};
        const handlerName = events[name];

        if (handlerName && typeof window[handlerName] === 'function') {

            try {
                window[handlerName](payload, instance.api);
            }
            catch (err) {
                console.error('TechUI map handler error:', err);
            }
        }

        Tech.trigger(
            instance.element,
            name.charAt(0).toUpperCase() + name.slice(1),
            payload);

        const listeners = instance.listeners[name];

        if (listeners) {

            listeners.slice().forEach(function (listener) {

                try {
                    listener(payload, instance.api);
                }
                catch (err) {
                    console.error('TechUI map listener error:', err);
                }
            });
        }
    }

    function addListener(instance, name, handler) {

        if (!name || typeof handler !== 'function') return;

        const list = instance.listeners[name] ||
            (instance.listeners[name] = []);

        list.push(handler);
    }

    function removeListener(instance, name, handler) {

        const list = instance.listeners[name];

        if (!list) return;

        const index = list.indexOf(handler);

        if (index >= 0) list.splice(index, 1);
    }

    // --------------------------------------------------------
    // View state
    // --------------------------------------------------------

    function cancelAnimation(instance) {

        if (instance.animation) {

            cancelAnimationFrame(instance.animation);
            instance.animation = null;
        }
    }

    function settleNow(instance) {

        clearTimeout(instance.settleTimer);
        instance.settleTimer = null;

        emit(instance, 'moveEnd', viewState(instance));

        if (instance.pendingZoomEvent) {

            instance.pendingZoomEvent = false;

            emit(instance, 'zoomEnd', viewState(instance));
        }
    }

    function markViewChanged(instance, zoomChanged, delay) {

        if (zoomChanged) instance.pendingZoomEvent = true;

        clearTimeout(instance.settleTimer);

        instance.settleTimer = setTimeout(function () {

            instance.settleTimer = null;

            emit(instance, 'moveEnd', viewState(instance));

            if (instance.pendingZoomEvent) {

                instance.pendingZoomEvent = false;

                emit(instance, 'zoomEnd', viewState(instance));
            }
        }, delay === undefined ? 180 : delay);
    }

    function commitViewOffset(instance) {

        if (!instance.viewOffset.x && !instance.viewOffset.y) return;

        const center = project(
            instance.center.lat, instance.center.lng, instance.zoom);

        const target = unproject(
            center.x - instance.viewOffset.x,
            center.y - instance.viewOffset.y,
            instance.zoom);

        instance.center = { lat: target.lat, lng: target.lng };
        instance.viewOffset = { x: 0, y: 0 };

        render(instance);
    }

    function animateView(instance, target, duration) {

        cancelAnimation(instance);

        const from = {
            lat: instance.center.lat,
            lng: instance.center.lng,
            zoom: instance.zoom
        };

        const deltaLng = shortestLngDelta(from.lng, target.lng);
        const start = window.performance && performance.now
            ? performance.now()
            : Date.now();

        duration = duration || 260;

        instance.pendingZoomEvent =
            Math.abs(target.zoom - from.zoom) > 0.0001;
        instance.viewOffset = { x: 0, y: 0 };

        function step(now) {

            const raw = Math.min(1, (now - start) / duration);
            const eased = 1 - Math.pow(1 - raw, 3);

            instance.center = {
                lat: from.lat + (target.lat - from.lat) * eased,
                lng: wrapLng(from.lng + deltaLng * eased)
            };

            instance.zoom = from.zoom + (target.zoom - from.zoom) * eased;

            render(instance);

            if (raw < 1) {
                instance.animation = requestAnimationFrame(step);
            }
            else {
                instance.animation = null;
                settleNow(instance);
            }
        }

        instance.animation = requestAnimationFrame(step);
    }

    function applyView(instance, latlng, zoom, animate) {

        cancelAnimation(instance);

        const target = {
            lat: clampLat(number(latlng && latlng.lat, instance.center.lat)),
            lng: wrapLng(number(latlng && latlng.lng, instance.center.lng)),
            zoom: clamp(
                number(zoom, instance.zoom),
                instance.minZoom,
                instance.maxZoom)
        };

        if (animate) {

            animateView(
                instance,
                target,
                typeof animate === 'number' ? animate : 260);

            return;
        }

        instance.pendingZoomEvent =
            Math.abs(target.zoom - instance.zoom) > 0.0001;
        instance.center = { lat: target.lat, lng: target.lng };
        instance.zoom = target.zoom;
        instance.viewOffset = { x: 0, y: 0 };

        render(instance);
        settleNow(instance);
    }

    function zoomAround(instance, anchorX, anchorY, zoom) {

        const targetZoom = clamp(
            zoom, instance.minZoom, instance.maxZoom);

        const latlng = screenToLatLng(instance, anchorX, anchorY);
        const anchor = project(latlng.lat, latlng.lng, targetZoom);

        const center = unproject(
            anchor.x + (instance.width / 2 - anchorX),
            anchor.y + (instance.height / 2 - anchorY),
            targetZoom);

        instance.pendingZoomEvent =
            Math.abs(targetZoom - instance.zoom) > 0.0001;
        instance.zoom = targetZoom;
        instance.center = { lat: center.lat, lng: center.lng };
        instance.viewOffset = { x: 0, y: 0 };

        render(instance);
    }

    function zoomAroundAnimated(instance, anchorX, anchorY, zoom) {

        const targetZoom = clamp(zoom, instance.minZoom, instance.maxZoom);
        const latlng = screenToLatLng(instance, anchorX, anchorY);
        const anchor = project(latlng.lat, latlng.lng, targetZoom);

        const center = unproject(
            anchor.x + (instance.width / 2 - anchorX),
            anchor.y + (instance.height / 2 - anchorY),
            targetZoom);

        animateView(instance, {
            lat: center.lat,
            lng: center.lng,
            zoom: targetZoom
        }, 220);
    }

    function panByPixels(instance, dx, dy, animate) {

        const center = project(
            instance.center.lat, instance.center.lng, instance.zoom);

        const target = unproject(
            center.x + dx, center.y + dy, instance.zoom);

        applyView(instance, target, instance.zoom, animate);
    }

    function zoomIn(instance) {

        const target = Math.min(
            instance.maxZoom,
            Math.floor(instance.zoom + 0.001) + 1);

        animateView(instance, {
            lat: instance.center.lat,
            lng: instance.center.lng,
            zoom: target
        }, 220);
    }

    function zoomOut(instance) {

        const target = Math.max(
            instance.minZoom,
            Math.ceil(instance.zoom - 0.001) - 1);

        animateView(instance, {
            lat: instance.center.lat,
            lng: instance.center.lng,
            zoom: target
        }, 220);
    }

    // --------------------------------------------------------
    // Markers
    // --------------------------------------------------------

    function addMarkerRecord(instance, config) {

        config = config || {};

        let id = config.id
            ? String(config.id)
            : 'marker-' + (++instance.markerCount);

        while (instance.markers.has(id)) {

            id = id + '-' + (++instance.markerCount);
        }

        const marker = {
            id: id,
            lat: clampLat(number(config.lat, instance.center.lat)),
            lng: wrapLng(number(config.lng, instance.center.lng)),
            title: config.title ? String(config.title) : '',
            popup: config.popup ? String(config.popup) : '',
            icon: String(config.icon || 'geo-alt-fill')
                .replace(/^bi-/, ''),
            tone: TONES.indexOf(config.tone) >= 0
                ? config.tone
                : 'primary',
            draggable: !!config.draggable,
            initialOpen: !!config.open,
            dragged: false,
            element: null,
            iconElement: null
        };

        const button = document.createElement('button');

        button.type = 'button';
        button.className = 'tech-map-marker tech-map-marker-' + marker.tone;
        button.dataset.markerId = marker.id;
        button.setAttribute(
            'aria-label', marker.title || 'نشانگر نقشه');
        button.setAttribute('aria-expanded', 'false');

        if (marker.title) {
            button.title = marker.title;
        }

        const pin = document.createElement('span');

        pin.className = 'tech-map-marker-pin';

        const icon = document.createElement('i');

        icon.className = 'bi bi-' + marker.icon;
        icon.setAttribute('aria-hidden', 'true');

        pin.appendChild(icon);
        button.appendChild(pin);

        marker.element = button;
        marker.iconElement = icon;

        button.addEventListener('click', function (event) {

            event.stopPropagation();

            if (marker.dragged) {

                marker.dragged = false;

                return;
            }

            if (marker.popup) {
                openPopup(instance, marker.id);
            }

            emit(instance, 'markerClick', {
                id: marker.id,
                lat: marker.lat,
                lng: marker.lng,
                title: marker.title
            });
        });

        if (marker.draggable) {
            wireMarkerDrag(instance, marker);
        }

        instance.markers.set(marker.id, marker);
        instance.markersLayer.appendChild(button);

        const handle = {
            id: marker.id,
            element: button,
            setLatLng: function (lat, lng) {

                marker.lat = clampLat(number(lat, marker.lat));
                marker.lng = wrapLng(number(lng, marker.lng));

                render(instance);

                return handle;
            },
            getLatLng: function () {

                return { lat: marker.lat, lng: marker.lng };
            },
            setIcon: function (name) {

                marker.icon = String(name || 'geo-alt-fill')
                    .replace(/^bi-/, '');
                marker.iconElement.className = 'bi bi-' + marker.icon;

                return handle;
            },
            openPopup: function () {

                openPopup(instance, marker.id);

                return handle;
            },
            closePopup: function () {

                closePopup(instance);

                return handle;
            },
            remove: function () {

                removeMarkerRecord(instance, marker.id);
            }
        };

        marker.handle = handle;

        return handle;
    }

    function wireMarkerDrag(instance, marker) {

        const button = marker.element;
        let dragState = null;

        button.addEventListener('pointerdown', function (event) {

            if (event.button !== undefined && event.button !== 0) return;

            event.preventDefault();
            event.stopPropagation();

            closePopup(instance);

            const start = project(
                marker.lat, marker.lng, instance.zoom);

            const origin = { x: event.clientX, y: event.clientY };

            dragState = { moved: false };

            button.classList.add('is-dragging');

            try {
                button.setPointerCapture(event.pointerId);
            }
            catch {
                // Pointer capture is a progressive enhancement.
            }

            function onMove(moveEvent) {

                const dx = moveEvent.clientX - origin.x;
                const dy = moveEvent.clientY - origin.y;

                if (!dragState.moved &&
                    Math.abs(dx) + Math.abs(dy) > 3) {

                    dragState.moved = true;
                }

                if (!dragState.moved) return;

                const target = unproject(
                    start.x + dx, start.y + dy, instance.zoom);

                marker.lat = target.lat;
                marker.lng = target.lng;

                render(instance);
            }

            function onUp() {

                button.removeEventListener('pointermove', onMove);
                button.removeEventListener('pointerup', onUp);
                button.removeEventListener('pointercancel', onUp);
                button.classList.remove('is-dragging');

                const moved = dragState ? dragState.moved : false;

                dragState = null;
                marker.dragged = moved;

                if (moved) {

                    emit(instance, 'markerDragEnd', {
                        id: marker.id,
                        lat: marker.lat,
                        lng: marker.lng
                    });
                }
            }

            button.addEventListener('pointermove', onMove);
            button.addEventListener('pointerup', onUp);
            button.addEventListener('pointercancel', onUp);
        });
    }

    function removeMarkerRecord(instance, id) {

        const marker = instance.markers.get(id);

        if (!marker) return false;

        if (instance.popupMarkerId === id) {
            closePopup(instance);
        }

        marker.element.remove();
        instance.markers.delete(id);

        return true;
    }

    function openPopup(instance, markerId) {

        const marker = instance.markers.get(markerId);

        if (!marker) return;

        instance.popupMarkerId = markerId;
        instance.popupContent.innerHTML = marker.popup || '';
        instance.popup.hidden = false;

        instance.markers.forEach(function (item) {

            item.element.setAttribute(
                'aria-expanded',
                item.id === markerId ? 'true' : 'false');
        });

        positionPopup(instance);
    }

    function closePopup(instance) {

        if (!instance.popupMarkerId) {

            instance.popup.hidden = true;

            return;
        }

        const marker = instance.markers.get(instance.popupMarkerId);

        if (marker) {
            marker.element.setAttribute('aria-expanded', 'false');
        }

        instance.popupMarkerId = null;
        instance.popup.hidden = true;
        instance.popupContent.innerHTML = '';
    }

    function positionPopup(instance) {

        if (!instance.popupMarkerId) return;

        const marker = instance.markers.get(instance.popupMarkerId);

        if (!marker) {
            closePopup(instance);

            return;
        }

        const point = screenPoint(instance, marker.lat, marker.lng);
        const popup = instance.popup;
        const padding = 10;
        const half = popup.offsetWidth / 2;

        let left = point.x;

        if (left - half < padding) left = half + padding;
        if (left + half > instance.width - padding) {
            left = instance.width - half - padding;
        }

        const below = point.y < popup.offsetHeight + 34;
        let top = point.y;

        if (below) {

            top = Math.min(
                top,
                instance.height - popup.offsetHeight - padding);
        }

        popup.style.left = left + 'px';
        popup.style.top = top + 'px';
        popup.classList.toggle('tech-map-popup-below', below);
    }

    // --------------------------------------------------------
    // Locate
    // --------------------------------------------------------

    function showLocateMarker(instance, lat, lng, accuracy) {

        if (!instance.locate) {

            const element = document.createElement('div');

            element.className = 'tech-map-locate-marker';
            element.setAttribute('aria-hidden', 'true');
            element.innerHTML =
                '<span class="tech-map-locate-accuracy"></span>' +
                '<span class="tech-map-locate-dot"></span>';

            instance.viewport.appendChild(element);
            instance.locate = { element: element };
        }

        instance.locate.lat = lat;
        instance.locate.lng = lng;
        instance.locate.accuracy = accuracy;
        instance.locate.element.hidden = false;

        updateLocateMarker(instance);
    }

    function updateLocateMarker(instance) {

        if (!instance.locate) return;

        const point = screenPoint(
            instance, instance.locate.lat, instance.locate.lng);

        instance.locate.element.style.transform =
            'translate3d(' + point.x + 'px,' + point.y + 'px,0)';

        const accuracy = number(instance.locate.accuracy, 0);
        const diameter = accuracy > 0
            ? accuracy / Math.max(
                0.01,
                metersPerPixel(instance.center.lat, instance.zoom)) * 2
            : 0;

        instance.locate.element.style.setProperty(
            '--tech-map-accuracy', Math.round(diameter) + 'px');
    }

    function locate(instance) {

        if (!navigator.geolocation) {

            Tech.notify(
                'مرورگر از موقعیت‌یابی پشتیبانی نمی‌کند.',
                { type: 'warning' });

            return Promise.resolve(null);
        }

        if (instance.locateButton) {
            instance.locateButton.classList.add('is-busy');
        }

        return new Promise(function (resolve) {

            navigator.geolocation.getCurrentPosition(
                function (position) {

                    if (instance.locateButton) {
                        instance.locateButton.classList.remove('is-busy');
                    }

                    const lat = position.coords.latitude;
                    const lng = position.coords.longitude;
                    const accuracy = position.coords.accuracy || 0;

                    showLocateMarker(instance, lat, lng, accuracy);
                    applyView(
                        instance,
                        { lat: lat, lng: lng },
                        Math.max(instance.zoom, 15),
                        true);

                    emit(instance, 'locate', {
                        lat: lat,
                        lng: lng,
                        accuracy: accuracy
                    });

                    resolve({ lat: lat, lng: lng, accuracy: accuracy });
                },
                function () {

                    if (instance.locateButton) {
                        instance.locateButton.classList.remove('is-busy');
                    }

                    Tech.notify(
                        'دسترسی به موقعیت مکانی ممکن نیست.',
                        { type: 'danger' });

                    emit(instance, 'locate', { error: 'unavailable' });

                    resolve(null);
                },
                {
                    enableHighAccuracy: true,
                    timeout: 10000
                });
        });
    }

    // --------------------------------------------------------
    // Search
    // --------------------------------------------------------

    function hideSearchResults(instance) {

        if (instance.searchResults) {
            instance.searchResults.hidden = true;
        }
    }

    function showSearchStatus(instance, message) {

        const list = instance.searchResults;

        if (!list) return;

        list.innerHTML = '';

        const status = document.createElement('div');

        status.className = 'tech-map-search-empty';
        status.textContent = message;

        list.appendChild(status);
        list.hidden = false;
    }

    function runSearch(instance, query) {

        const list = instance.searchResults;

        if (!list || !query) return;

        if (instance.searchController) {
            instance.searchController.abort();
        }

        instance.searchController = new AbortController();

        showSearchStatus(instance, 'در حال جستجو…');

        let url;

        try {

            url = new URL(
                instance.options.searchUrl, window.location.href);
        }
        catch {

            showSearchStatus(instance, 'آدرس جستجو معتبر نیست.');

            return;
        }

        url.searchParams.set('format', 'jsonv2');
        url.searchParams.set('limit', '6');
        url.searchParams.set('accept-language', 'fa');
        url.searchParams.set('q', query);

        fetch(url.toString(), {
            signal: instance.searchController.signal,
            headers: { Accept: 'application/json' }
        })
            .then(function (response) {

                if (!response.ok) {
                    throw new Error('HTTP ' + response.status);
                }

                return response.json();
            })
            .then(function (data) {

                instance.searchController = null;

                const results = Array.isArray(data) ? data : [];

                renderSearchResults(instance, results, query);

                emit(instance, 'search', {
                    query: query,
                    results: results.map(function (result) {

                        return {
                            name: result.name || '',
                            displayName: result.display_name || '',
                            lat: number(result.lat, 0),
                            lng: number(result.lon, 0)
                        };
                    })
                });
            })
            .catch(function (err) {

                if (err && err.name === 'AbortError') return;

                instance.searchController = null;

                showSearchStatus(
                    instance,
                    'جستجو ناموفق بود. اتصال اینترنت را بررسی کنید.');
            });
    }

    function renderSearchResults(instance, results, query) {

        const list = instance.searchResults;

        if (!list) return;

        list.innerHTML = '';

        if (!results.length) {

            showSearchStatus(instance, 'نتیجه‌ای پیدا نشد.');

            return;
        }

        results.forEach(function (result) {

            const lat = number(result.lat, NaN);
            const lng = number(result.lon, NaN);

            if (!isFinite(lat) || !isFinite(lng)) return;

            const item = document.createElement('button');

            item.type = 'button';
            item.className = 'tech-map-search-item';

            const title = document.createElement('span');

            title.className = 'tech-map-search-item-title';
            title.textContent = result.name ||
                String(result.display_name || '').split(',')[0] ||
                query;

            const subtitle = document.createElement('span');

            subtitle.className = 'tech-map-search-item-sub';
            subtitle.textContent = result.display_name || '';

            item.appendChild(title);
            item.appendChild(subtitle);

            item.addEventListener('click', function () {

                list.hidden = true;

                selectSearchResult(instance, result, {
                    lat: lat,
                    lng: lng
                });
            });

            list.appendChild(item);
        });

        list.hidden = false;
    }

    function selectSearchResult(instance, result, latlng) {

        removeMarkerRecord(instance, '__search');

        const name = result.name ||
            String(result.display_name || '').split(',')[0] ||
            'نتیجه جستجو';

        const popup = '<div class="tech-map-popup-title">' +
            escapeHtml(name) + '</div>' +
            '<div class="tech-map-popup-text">' +
            escapeHtml(result.display_name || '') + '</div>';

        const handle = addMarkerRecord(instance, {
            id: '__search',
            lat: latlng.lat,
            lng: latlng.lng,
            title: name,
            popup: popup,
            icon: 'search',
            tone: 'danger'
        });

        render(instance);

        const box = Array.isArray(result.boundingbox)
            ? result.boundingbox.map(Number)
            : null;

        const hasBox = box && box.length === 4 &&
            box.every(function (value) { return isFinite(value); }) &&
            (box[1] - box[0] > 0.0005 || box[3] - box[2] > 0.0005);

        if (hasBox) {

            fitBounds(instance, [
                [box[0], box[2]],
                [box[1], box[3]]
            ], { maxZoom: 16 });
        }
        else {

            applyView(
                instance,
                latlng,
                Math.max(instance.zoom, 15),
                true);
        }

        openPopup(instance, handle.id);
    }

    function fitBounds(instance, points, options) {

        const items = (points || []).map(function (point) {

            if (Array.isArray(point)) {

                return { lat: number(point[0], 0), lng: number(point[1], 0) };
            }

            return point || {};
        });

        const valid = items.filter(function (item) {

            return isFinite(number(item.lat, NaN)) &&
                isFinite(number(item.lng, NaN));
        });

        if (!valid.length) return;

        const maxZoom = number(options && options.maxZoom, instance.maxZoom);

        if (valid.length === 1) {

            applyView(
                instance,
                valid[0],
                Math.min(maxZoom, 15),
                true);

            return;
        }

        let south = 90, north = -90, west = 180, east = -180;

        valid.forEach(function (item) {

            south = Math.min(south, item.lat);
            north = Math.max(north, item.lat);
            west = Math.min(west, item.lng);
            east = Math.max(east, item.lng);
        });

        const southWest = project(south, west, 0);
        const northEast = project(north, east, 0);

        const worldWidth = Math.abs(northEast.x - southWest.x);
        const worldHeight = Math.abs(northEast.y - southWest.y);

        if (worldWidth < 0.000001 && worldHeight < 0.000001) {

            applyView(instance, {
                lat: (south + north) / 2,
                lng: (west + east) / 2
            }, Math.min(maxZoom, 15), true);

            return;
        }

        const zoomX = worldWidth >= 0.000001
            ? Math.log2((instance.width * 0.85) / worldWidth)
            : Infinity;

        const zoomY = worldHeight >= 0.000001
            ? Math.log2((instance.height * 0.85) / worldHeight)
            : Infinity;

        const zoom = clamp(
            Math.floor(Math.min(zoomX, zoomY)),
            instance.minZoom,
            maxZoom);

        applyView(instance, {
            lat: (south + north) / 2,
            lng: (west + east) / 2
        }, zoom, true);
    }

    // --------------------------------------------------------
    // Base layers
    // --------------------------------------------------------

    function updateLayerButtons(instance) {

        (instance.layerButtons || []).forEach(function (entry) {

            const active = entry.index === instance.activeIndex;

            entry.element.classList.toggle('is-active', active);
            entry.element.setAttribute(
                'aria-pressed', active ? 'true' : 'false');
        });
    }

    function setBaseLayer(instance, value) {

        let index = -1;

        if (typeof value === 'number') {

            index = value;
        }
        else {

            const text = String(value === null || value === undefined
                ? ''
                : value);

            index = instance.layers.findIndex(function (layer, i) {

                return String(i) === text ||
                    layer.name === text ||
                    layer.url === text;
            });
        }

        if (index < 0 || index >= instance.layers.length) return false;

        instance.activeIndex = index;
        instance.tilesLayer.innerHTML = '';
        instance.tiles.clear();
        instance.tileZoom = null;

        instance.attribution.innerHTML =
            instance.layers[index].attribution || '';
        instance.attribution.hidden = !instance.layers[index].attribution;

        const limit = instance.layers[index].maxZoom;

        instance.maxZoom = Math.max(
            instance.minZoom,
            Math.min(
                number(instance.options.maxZoom, 22),
                limit || 22));

        updateLayerButtons(instance);

        if (instance.zoom > instance.maxZoom) {
            applyView(instance, instance.center, instance.maxZoom, false);
        }
        else {
            render(instance);
        }

        return true;
    }

    // --------------------------------------------------------
    // Render
    // --------------------------------------------------------

    function renderTiles(instance, tileZoom, scale, offsetX, offsetY) {

        const count = Math.pow(2, tileZoom);
        const left = offsetX / scale;
        const top = offsetY / scale;
        const right = (offsetX + instance.width) / scale;
        const bottom = (offsetY + instance.height) / scale;

        const minX = Math.floor(left / TILE_SIZE);
        const maxX = Math.floor((right - 1) / TILE_SIZE);
        const minY = Math.max(0, Math.floor(top / TILE_SIZE));
        const maxY = Math.min(
            count - 1,
            Math.floor((bottom - 1) / TILE_SIZE));

        instance.tiles.forEach(function (tile) {
            tile.used = false;
        });

        if (maxY < minY) return;

        for (let y = minY; y <= maxY; y++) {

            for (let x = minX; x <= maxX; x++) {

                const wrappedX = ((x % count) + count) % count;
                const key = wrappedX + ':' + y + ':' + tileZoom;

                let tile = instance.tiles.get(key);

                if (!tile) {

                    const img = document.createElement('img');

                    img.className = 'tech-map-tile';
                    img.alt = '';
                    img.draggable = false;
                    img.decoding = 'async';
                    img.src = tileUrl(
                        instance, wrappedX, y, tileZoom);

                    if (img.complete) {
                        img.classList.add('is-loaded');
                    }

                    img.addEventListener('load', function () {
                        img.classList.add('is-loaded');
                    });

                    tile = { element: img, used: true };

                    instance.tiles.set(key, tile);
                    instance.tilesLayer.appendChild(img);
                }

                tile.used = true;
                tile.element.style.left = (x * TILE_SIZE) + 'px';
                tile.element.style.top = (y * TILE_SIZE) + 'px';
            }
        }

        instance.tiles.forEach(function (tile, key) {

            if (tile.used) return;

            tile.element.remove();
            instance.tiles.delete(key);
        });
    }

    function buildShapes(instance) {

        instance.shapes = [];
        instance.shapesLayer.innerHTML = '';

        (instance.options.polylines || []).forEach(function (shape) {

            const element = document.createElementNS(SVG_NS, 'polyline');

            element.setAttribute('fill', 'none');
            element.setAttribute('stroke-linecap', 'round');
            element.setAttribute('stroke-linejoin', 'round');

            instance.shapesLayer.appendChild(element);
            instance.shapes.push({
                type: 'polyline',
                shape: shape,
                element: element
            });
        });

        (instance.options.polygons || []).forEach(function (shape) {

            const element = document.createElementNS(SVG_NS, 'polygon');

            element.setAttribute('stroke-linejoin', 'round');
            element.setAttribute(
                'fill-opacity',
                String(clamp(number(shape.fillOpacity, 0.18), 0, 1)));

            instance.shapesLayer.appendChild(element);
            instance.shapes.push({
                type: 'polygon',
                shape: shape,
                element: element
            });
        });

        (instance.options.circles || []).forEach(function (circle) {

            const element = document.createElementNS(SVG_NS, 'circle');

            element.setAttribute(
                'fill-opacity',
                String(clamp(number(circle.fillOpacity, 0.15), 0, 1)));

            instance.shapesLayer.appendChild(element);
            instance.shapes.push({
                type: 'circle',
                circle: circle,
                element: element
            });
        });
    }

    function renderShapes(instance) {

        instance.shapes.forEach(function (record) {

            if (record.type === 'circle') {

                const circle = record.circle;
                const point = screenPoint(
                    instance,
                    number(circle.lat, 0),
                    number(circle.lng, 0));

                const radius = Math.max(
                    1,
                    number(circle.radius, 0) /
                    Math.max(
                        0.01,
                        metersPerPixel(instance.center.lat, instance.zoom)));

                const color = circle.color || '#2563eb';

                record.element.setAttribute('cx', String(point.x));
                record.element.setAttribute('cy', String(point.y));
                record.element.setAttribute('r', String(radius));
                record.element.style.stroke = color;
                record.element.style.fill = color;

                return;
            }

            const shape = record.shape;
            const points = (shape.points || []).map(function (point) {

                const screen = screenPoint(
                    instance,
                    number(point[0], 0),
                    number(point[1], 0));

                return screen.x.toFixed(1) + ',' + screen.y.toFixed(1);
            }).join(' ');

            const color = shape.color || '#2563eb';

            record.element.setAttribute('points', points);
            record.element.style.stroke = color;
            record.element.style.strokeWidth =
                number(shape.weight, 3) + 'px';
            record.element.style.strokeOpacity =
                String(clamp(number(shape.opacity, 0.9), 0, 1));

            if (record.type === 'polygon') {
                record.element.style.fill = color;
            }
        });
    }

    function updateScale(instance) {

        if (!instance.scaleBar) return;

        const mpp = metersPerPixel(
            instance.center.lat, instance.zoom);

        if (!isFinite(mpp) || mpp <= 0) return;

        const target = mpp * 84;
        let meters = SCALE_STEPS[0];

        SCALE_STEPS.forEach(function (step) {

            if (step <= target) meters = step;
        });

        const width = clamp(meters / mpp, 24, 140);

        instance.scaleBar.style.width = Math.round(width) + 'px';
        instance.scaleLabel.textContent = meters >= 1000
            ? (meters / 1000) + ' km'
            : meters + ' m';
    }

    function updateCoordinates(instance, event) {

        if (!instance.coords) return;

        const point = localPoint(instance, event);
        const latlng = screenToLatLng(instance, point.x, point.y);

        instance.coordsValue.textContent =
            latlng.lat.toFixed(5) + '° , ' + latlng.lng.toFixed(5) + '°';
    }

    function render(instance) {

        if (instance.destroyed) return;

        const width = Math.max(1, instance.viewport.clientWidth);
        const height = Math.max(1, instance.viewport.clientHeight);

        instance.width = width;
        instance.height = height;

        const zoom = instance.zoom;
        const tileZoom = clamp(Math.round(zoom), 0, 22);
        const scale = Math.pow(2, zoom - tileZoom);

        if (instance.tileZoom !== tileZoom ||
            instance.tileLayerKey !== instance.activeIndex) {

            instance.tilesLayer.innerHTML = '';
            instance.tiles.clear();
            instance.tileZoom = tileZoom;
            instance.tileLayerKey = instance.activeIndex;
        }

        const center = project(
            instance.center.lat, instance.center.lng, tileZoom);

        const offsetX =
            center.x * scale - width / 2 - instance.viewOffset.x;
        const offsetY =
            center.y * scale - height / 2 - instance.viewOffset.y;

        instance.tilesLayer.style.transform =
            'translate3d(' + (-offsetX) + 'px,' + (-offsetY) + 'px,0) ' +
            'scale(' + scale + ')';

        renderTiles(instance, tileZoom, scale, offsetX, offsetY);

        instance.markers.forEach(function (marker) {

            const point = screenPoint(instance, marker.lat, marker.lng);

            marker.element.style.transform =
                'translate3d(' + point.x + 'px,' + point.y + 'px,0) ' +
                'translate(-50%,-100%)';

            marker.element.classList.toggle(
                'is-outside',
                point.x < -60 || point.y < -80 ||
                point.x > width + 60 || point.y > height + 60);
        });

        renderShapes(instance);
        positionPopup(instance);
        updateScale(instance);
        updateLocateMarker(instance);
    }

    // --------------------------------------------------------
    // Init
    // --------------------------------------------------------

    function initMap(root) {

        if (!root || root._techMap) {

            return root && root._techMapInstance
                ? root._techMapInstance.api
                : null;
        }

        const options = parseMapOptions(root);
        const controls = options.controls || {};
        const layers = normalizeLayers(options);

        let minZoom = clamp(number(options.minZoom, 2), 1, 22);
        let maxZoom = clamp(number(options.maxZoom, 19), 1, 22);

        if (minZoom > maxZoom) {

            const swap = minZoom;

            minZoom = maxZoom;
            maxZoom = swap;
        }

        const instance = {
            element: root,
            options: options,
            minZoom: minZoom,
            maxZoom: maxZoom,
            center: {
                lat: clampLat(number(
                    options.center && options.center.lat, 0)),
                lng: wrapLng(number(
                    options.center && options.center.lng, 0))
            },
            zoom: clamp(number(options.zoom, 12), minZoom, maxZoom),
            width: 1,
            height: 1,
            viewOffset: { x: 0, y: 0 },
            layers: layers,
            activeIndex: 0,
            markers: new Map(),
            markerCount: 0,
            tiles: new Map(),
            tileZoom: null,
            tileLayerKey: null,
            shapes: [],
            popupMarkerId: null,
            locate: null,
            pointers: new Map(),
            listeners: {},
            pendingZoomEvent: false,
            settleTimer: null,
            animation: null,
            destroyed: false
        };

        const selected = layers.findIndex(function (layer) {
            return layer.selected;
        });

        instance.activeIndex = selected >= 0 ? selected : 0;

        const initialLayer = layers[instance.activeIndex];

        if (initialLayer && initialLayer.maxZoom) {

            instance.maxZoom = Math.max(
                instance.minZoom,
                Math.min(instance.maxZoom, initialLayer.maxZoom));
            instance.zoom = Math.min(instance.zoom, instance.maxZoom);
        }

        root.innerHTML = '';
        root._techMap = true;

        const viewport = document.createElement('div');

        viewport.className = 'tech-map-viewport';
        viewport.tabIndex = 0;
        viewport.setAttribute('role', 'region');
        viewport.setAttribute('aria-roledescription', 'نقشه');
        viewport.setAttribute(
            'aria-label',
            root.getAttribute('aria-label') || 'نقشه تعاملی');

        instance.viewport = viewport;

        const tilesLayer = document.createElement('div');

        tilesLayer.className = 'tech-map-tiles';
        tilesLayer.setAttribute('aria-hidden', 'true');

        const shapesLayer = document.createElementNS(SVG_NS, 'svg');

        shapesLayer.setAttribute('class', 'tech-map-shapes');
        shapesLayer.setAttribute('width', '100%');
        shapesLayer.setAttribute('height', '100%');
        shapesLayer.setAttribute('aria-hidden', 'true');

        const markersLayer = document.createElement('div');

        markersLayer.className = 'tech-map-markers';

        const popup = document.createElement('div');

        popup.className = 'tech-map-popup';
        popup.hidden = true;
        popup.innerHTML =
            '<button type="button" class="tech-map-popup-close" ' +
            'aria-label="بستن پنجره">' +
            '<i class="bi bi-x-lg" aria-hidden="true"></i></button>' +
            '<div class="tech-map-popup-content"></div>';

        const controlsStart = document.createElement('div');

        controlsStart.className =
            'tech-map-controls tech-map-controls-start';

        const controlsEnd = document.createElement('div');

        controlsEnd.className =
            'tech-map-controls tech-map-controls-end';

        const attribution = document.createElement('div');

        attribution.className = 'tech-map-attribution';
        attribution.innerHTML =
            instance.layers[instance.activeIndex].attribution || '';
        attribution.hidden =
            !instance.layers[instance.activeIndex].attribution;

        instance.tilesLayer = tilesLayer;
        instance.shapesLayer = shapesLayer;
        instance.markersLayer = markersLayer;
        instance.popup = popup;
        instance.popupContent =
            popup.querySelector('.tech-map-popup-content');
        instance.attribution = attribution;

        viewport.appendChild(tilesLayer);
        viewport.appendChild(shapesLayer);
        viewport.appendChild(markersLayer);
        viewport.appendChild(popup);

        // Controls

        if (controls.search && options.searchUrl) {
            buildSearch(instance, controlsStart);
        }

        if (controls.zoom !== false) {

            const group = createControlGroup();
            const zoomInButton = createButton('plus-lg', 'بزرگ‌نمایی');
            const zoomOutButton = createButton('dash-lg', 'کوچک‌نمایی');

            zoomInButton.addEventListener('click', function () {
                zoomIn(instance);
            });

            zoomOutButton.addEventListener('click', function () {
                zoomOut(instance);
            });

            group.appendChild(zoomInButton);
            group.appendChild(zoomOutButton);
            controlsStart.appendChild(group);
        }

        const endGroup = createControlGroup();

        if (controls.locate !== false && navigator.geolocation) {

            const locateButton = createButton(
                'crosshair', 'موقعیت من');

            locateButton.addEventListener('click', function () {
                locate(instance);
            });

            instance.locateButton = locateButton;
            endGroup.appendChild(locateButton);
        }

        if (controls.fullscreen !== false &&
            (root.requestFullscreen || root.webkitRequestFullscreen)) {

            const fullscreenButton = createButton(
                'arrows-fullscreen', 'تمام‌صفحه');

            fullscreenButton.addEventListener('click', function () {
                toggleFullscreen(instance);
            });

            instance.fullscreenButton = fullscreenButton;
            endGroup.appendChild(fullscreenButton);
        }

        if (controls.layers && instance.layers.length > 1) {

            const layersButton = createButton('layers', 'لایه‌های نقشه');
            const layerList = document.createElement('div');

            layerList.className = 'tech-map-layer-list';
            layerList.hidden = true;

            instance.layerList = layerList;
            instance.layerButtons = [];

            instance.layers.forEach(function (layer, index) {

                const item = document.createElement('button');

                item.type = 'button';
                item.className = 'tech-map-layer-item';
                item.setAttribute(
                    'aria-pressed',
                    index === instance.activeIndex ? 'true' : 'false');

                const check = document.createElement('i');

                check.className = 'bi bi-check-lg tech-map-layer-check';
                check.setAttribute('aria-hidden', 'true');

                const name = document.createElement('span');

                name.textContent = layer.name;

                item.appendChild(check);
                item.appendChild(name);

                if (index === instance.activeIndex) {
                    item.classList.add('is-active');
                }

                item.addEventListener('click', function (event) {

                    event.stopPropagation();

                    setBaseLayer(instance, index);

                    layerList.hidden = true;
                });

                instance.layerButtons.push({
                    index: index,
                    element: item
                });

                layerList.appendChild(item);
            });

            layersButton.addEventListener('click', function (event) {

                event.stopPropagation();

                layerList.hidden = !layerList.hidden;
            });

            endGroup.appendChild(layersButton);
            controlsEnd.appendChild(endGroup);
            controlsEnd.appendChild(layerList);
        }
        else {

            controlsEnd.appendChild(endGroup);
        }

        if (endGroup.childElementCount) {
            controlsEnd.hidden = false;
        }

        // Overlay controls

        viewport.appendChild(controlsStart);
        viewport.appendChild(controlsEnd);

        if (controls.scale !== false) {

            const scale = document.createElement('div');

            scale.className = 'tech-map-scale';

            const bar = document.createElement('div');

            bar.className = 'tech-map-scale-bar';

            const label = document.createElement('span');

            label.className = 'tech-map-scale-label';

            scale.appendChild(bar);
            scale.appendChild(label);
            viewport.appendChild(scale);

            instance.scaleBar = bar;
            instance.scaleLabel = label;
        }

        if (controls.coordinates) {

            const coords = document.createElement('div');

            coords.className = 'tech-map-coords';

            const coordsValue = document.createElement('span');

            coordsValue.className = 'tech-map-coords-value';
            coordsValue.setAttribute('dir', 'ltr');
            coordsValue.textContent = '—';

            coords.appendChild(coordsValue);
            viewport.appendChild(coords);

            instance.coords = coords;
            instance.coordsValue = coordsValue;
        }

        viewport.appendChild(attribution);
        root.appendChild(viewport);

        // Popup click handling

        popup.addEventListener('click', function (event) {
            event.stopPropagation();
        });

        popup.querySelector('.tech-map-popup-close')
            .addEventListener('click', function (event) {

                event.stopPropagation();
                closePopup(instance);
            });

        // Shapes and markers

        buildShapes(instance);

        (options.markers || []).forEach(function (config) {

            addMarkerRecord(instance, {
                id: config.id,
                lat: config.lat,
                lng: config.lng,
                title: config.title,
                popup: config.popup,
                icon: config.icon,
                tone: config.tone,
                draggable: config.draggable,
                open: config.open
            });
        });

        // Pointer interactions

        let drag = null;
        let pinch = null;

        function startPinch() {

            const points = Array.from(instance.pointers.values());

            if (points.length < 2) return;

            const rect = viewport.getBoundingClientRect();

            pinch = {
                distance: Math.max(
                    1,
                    Math.hypot(
                        points[0].x - points[1].x,
                        points[0].y - points[1].y)),
                zoom: instance.zoom,
                x: (points[0].x + points[1].x) / 2 - rect.left,
                y: (points[0].y + points[1].y) / 2 - rect.top
            };
        }

        function updatePinch() {

            const points = Array.from(instance.pointers.values());

            if (!pinch || points.length < 2) return;

            const distance = Math.max(
                1,
                Math.hypot(
                    points[0].x - points[1].x,
                    points[0].y - points[1].y));

            const rect = viewport.getBoundingClientRect();
            const anchorX = (points[0].x + points[1].x) / 2 - rect.left;
            const anchorY = (points[0].y + points[1].y) / 2 - rect.top;
            const target = clamp(
                pinch.zoom + Math.log2(distance / pinch.distance),
                instance.minZoom,
                instance.maxZoom);

            zoomAround(instance, anchorX, anchorY, target);
            markViewChanged(instance, true, 200);
        }

        viewport.addEventListener('pointerdown', function (event) {

            if (event.pointerType === 'mouse' && event.button !== 0) {
                return;
            }

            if (event.target.closest &&
                event.target.closest(
                    '.tech-map-controls, .tech-map-popup, ' +
                    '.tech-map-marker, .tech-map-attribution')) {

                return;
            }

            try {
                viewport.focus({ preventScroll: true });
            }
            catch {
                // Older browsers ignore focus options.
            }

            hideSearchResults(instance);

            if (instance.layerList) {
                instance.layerList.hidden = true;
            }

            instance.pointers.set(event.pointerId, {
                x: event.clientX,
                y: event.clientY
            });

            try {
                viewport.setPointerCapture(event.pointerId);
            }
            catch {
                // Pointer capture is a progressive enhancement.
            }

            if (instance.pointers.size === 1) {

                if (instance.options.dragging === false) return;

                cancelAnimation(instance);

                drag = {
                    pointerId: event.pointerId,
                    x: event.clientX,
                    y: event.clientY,
                    moved: false
                };

                viewport.classList.add('is-dragging');
            }
            else if (instance.pointers.size === 2) {

                if (drag && drag.moved) {
                    commitViewOffset(instance);
                }

                drag = null;
                viewport.classList.remove('is-dragging');

                startPinch();
            }
        });

        viewport.addEventListener('pointermove', function (event) {

            updateCoordinates(instance, event);

            if (!instance.pointers.has(event.pointerId)) return;

            instance.pointers.set(event.pointerId, {
                x: event.clientX,
                y: event.clientY
            });

            if (pinch) {

                updatePinch();

                return;
            }

            if (!drag || drag.pointerId !== event.pointerId) return;

            const dx = event.clientX - drag.x;
            const dy = event.clientY - drag.y;

            if (!drag.moved &&
                Math.abs(dx) + Math.abs(dy) > 3) {

                drag.moved = true;
                closePopup(instance);
            }

            if (!drag.moved) return;

            instance.viewOffset.x += dx;
            instance.viewOffset.y += dy;
            drag.x = event.clientX;
            drag.y = event.clientY;

            render(instance);
        });

        function endPointer(event) {

            if (!instance.pointers.has(event.pointerId)) return;

            instance.pointers.delete(event.pointerId);

            if (pinch) {

                if (instance.pointers.size < 2) {

                    pinch = null;
                    settleNow(instance);
                }

                return;
            }

            if (!drag || drag.pointerId !== event.pointerId) return;

            const moved = drag.moved;
            const point = localPoint(instance, event);

            drag = null;
            viewport.classList.remove('is-dragging');

            if (moved) {

                commitViewOffset(instance);
                settleNow(instance);

                return;
            }

            const latlng = screenToLatLng(instance, point.x, point.y);

            closePopup(instance);

            emit(instance, 'click', {
                lat: latlng.lat,
                lng: latlng.lng,
                point: point
            });
        }

        viewport.addEventListener('pointerup', endPointer);

        viewport.addEventListener('pointercancel', function (event) {

            instance.pointers.delete(event.pointerId);

            if (pinch && instance.pointers.size < 2) {
                pinch = null;
            }

            if (drag && drag.pointerId === event.pointerId) {

                drag = null;
                viewport.classList.remove('is-dragging');

                commitViewOffset(instance);
            }
        });

        viewport.addEventListener('pointerleave', function () {

            if (instance.coordsValue) {
                instance.coordsValue.textContent = '—';
            }
        });

        viewport.addEventListener('wheel', function (event) {

            if (instance.options.scrollWheelZoom === false) return;

            if (event.target.closest &&
                event.target.closest('.tech-map-controls, .tech-map-popup')) {

                return;
            }

            event.preventDefault();

            const point = localPoint(instance, event);
            const delta = event.deltaMode === 1
                ? event.deltaY * 16
                : event.deltaY;
            const factor = Math.exp(-delta * 0.0016);

            zoomAround(
                instance,
                point.x,
                point.y,
                clamp(
                    instance.zoom + Math.log2(factor),
                    instance.minZoom,
                    instance.maxZoom));

            markViewChanged(instance, true, 220);
        }, { passive: false });

        viewport.addEventListener('dblclick', function (event) {

            if (instance.options.doubleClickZoom === false) return;

            if (event.target.closest &&
                event.target.closest(
                    '.tech-map-controls, .tech-map-marker, ' +
                    '.tech-map-popup')) {

                return;
            }

            event.preventDefault();

            const point = localPoint(instance, event);

            zoomAroundAnimated(
                instance,
                point.x,
                point.y,
                Math.min(
                    instance.maxZoom,
                    Math.floor(instance.zoom) + 1));
        });

        viewport.addEventListener('keydown', function (event) {

            if (event.altKey || event.ctrlKey || event.metaKey) return;

            const key = event.key;

            if (key === 'Escape') {

                closePopup(instance);

                if (instance.layerList) {
                    instance.layerList.hidden = true;
                }

                hideSearchResults(instance);

                return;
            }

            if (instance.options.keyboard === false) return;

            if (key === '+' || key === '=') {

                event.preventDefault();
                zoomIn(instance);

                return;
            }

            if (key === '-' || key === '_') {

                event.preventDefault();
                zoomOut(instance);

                return;
            }

            let dx = 0;
            let dy = 0;

            if (key === 'ArrowUp') dy = -100;
            else if (key === 'ArrowDown') dy = 100;
            else if (key === 'ArrowLeft') dx = -100;
            else if (key === 'ArrowRight') dx = 100;
            else return;

            event.preventDefault();

            panByPixels(instance, dx, dy, true);
        });

        // Fullscreen

        instance.onFullscreenChange = function () {

            const active = document.fullscreenElement === root ||
                document.webkitFullscreenElement === root;

            if (instance.fullscreenButton) {

                const icon = instance.fullscreenButton
                    .querySelector('i');

                if (icon) {
                    icon.className = 'bi bi-' + (active
                        ? 'fullscreen-exit'
                        : 'arrows-fullscreen');
                }

                instance.fullscreenButton.setAttribute(
                    'aria-label',
                    active ? 'خروج از تمام‌صفحه' : 'تمام‌صفحه');
            }

            render(instance);
        };

        document.addEventListener(
            'fullscreenchange', instance.onFullscreenChange);

        document.addEventListener(
            'webkitfullscreenchange', instance.onFullscreenChange);

        // API

        const api = {
            element: root,
            getCenter: function () {

                return {
                    lat: instance.center.lat,
                    lng: instance.center.lng
                };
            },
            getZoom: function () {
                return instance.zoom;
            },
            getBounds: function () {
                return viewState(instance).bounds;
            },
            setView: function (latlng, zoom, animate) {

                applyView(
                    instance,
                    latlng,
                    zoom === undefined ? instance.zoom : zoom,
                    animate);

                return api;
            },
            panTo: function (latlng, animate) {

                applyView(instance, latlng, instance.zoom, animate);

                return api;
            },
            setZoom: function (zoom) {

                applyView(instance, instance.center, zoom, false);

                return api;
            },
            zoomIn: function () {

                zoomIn(instance);

                return api;
            },
            zoomOut: function () {

                zoomOut(instance);

                return api;
            },
            panBy: function (dx, dy) {

                panByPixels(
                    instance,
                    number(dx, 0),
                    number(dy, 0),
                    false);

                return api;
            },
            addMarker: function (config) {

                const handle = addMarkerRecord(instance, config);

                render(instance);

                if (config && config.open) {
                    openPopup(instance, handle.id);
                }

                return handle;
            },
            removeMarker: function (id) {

                return removeMarkerRecord(instance, id);
            },
            getMarkers: function () {

                return Array.from(instance.markers.values())
                    .map(function (marker) {
                        return marker.handle;
                    });
            },
            setBaseLayer: function (value) {

                return setBaseLayer(instance, value);
            },
            getBaseLayer: function () {

                return instance.layers[instance.activeIndex];
            },
            locate: function () {
                return locate(instance);
            },
            fitBounds: function (points, fitOptions) {

                fitBounds(instance, points, fitOptions);

                return api;
            },
            invalidateSize: function () {

                render(instance);

                return api;
            },
            openPopup: function (id) {

                openPopup(instance, id);

                return api;
            },
            closePopup: function () {

                closePopup(instance);

                return api;
            },
            on: function (name, handler) {

                addListener(instance, name, handler);

                return api;
            },
            off: function (name, handler) {

                removeListener(instance, name, handler);

                return api;
            },
            destroy: function () {
                instance.destroy();
            }
        };

        instance.api = api;

        instance.destroy = function () {

            instance.destroyed = true;

            cancelAnimation(instance);
            clearTimeout(instance.settleTimer);

            if (instance.observer) instance.observer.disconnect();

            if (instance.onWindowResize) {

                window.removeEventListener(
                    'resize', instance.onWindowResize);
            }

            document.removeEventListener(
                'fullscreenchange', instance.onFullscreenChange);

            document.removeEventListener(
                'webkitfullscreenchange', instance.onFullscreenChange);

            if (instance.searchController) {
                instance.searchController.abort();
            }

            if (root.id) {
                Tech.Map.instances.delete(root.id);
            }

            root._techMap = false;
            root._techMapInstance = null;
            root.innerHTML = '';
        };

        // Observers

        if (window.ResizeObserver) {

            instance.observer = new ResizeObserver(function () {
                render(instance);
            });

            instance.observer.observe(root);
        }

        instance.onWindowResize = function () {
            render(instance);
        };

        window.addEventListener('resize', instance.onWindowResize);

        root._techMapInstance = instance;

        if (root.id) {
            Tech.Map.instances.set(root.id, instance);
        }

        render(instance);

        instance.markers.forEach(function (marker) {

            if (marker.initialOpen) {
                openPopup(instance, marker.id);
            }
        });

        return instance.api;
    }

    function normalizeLayers(options) {

        const source = Array.isArray(options.layers) ? options.layers : [];
        const layers = [];

        source.forEach(function (layer) {

            if (!layer || !layer.url) return;

            layers.push({
                name: layer.name || 'لایه',
                url: layer.url,
                attribution: layer.attribution || '',
                selected: !!layer.selected,
                maxZoom: number(layer.maxZoom, 0) || null,
                subdomains: Array.isArray(layer.subdomains)
                    ? layer.subdomains
                    : []
            });
        });

        if (!layers.length) {

            layers.push({
                name: 'نقشه پایه',
                url: options.tileUrl ||
                    'https://tile.openstreetmap.org/{z}/{x}/{y}.png',
                attribution: options.attribution || '',
                selected: true,
                maxZoom: null,
                subdomains: Array.isArray(options.subdomains)
                    ? options.subdomains
                    : []
            });
        }

        return layers;
    }

    function buildSearch(instance, controlsStart) {

        const wrap = document.createElement('div');

        wrap.className = 'tech-map-search-wrap';

        const form = document.createElement('form');

        form.className = 'tech-map-search';
        form.setAttribute('role', 'search');

        const icon = document.createElement('i');

        icon.className = 'bi bi-search tech-map-search-icon';
        icon.setAttribute('aria-hidden', 'true');

        const input = document.createElement('input');

        input.type = 'search';
        input.className = 'tech-map-search-input';
        input.placeholder = instance.options.searchPlaceholder ||
            'جستجوی مکان…';
        input.setAttribute('aria-label', 'جستجوی مکان');
        input.autocomplete = 'off';

        const submit = createButton(
            'arrow-return-left', 'جستجو', 'tech-map-search-submit');

        submit.type = 'submit';

        form.appendChild(icon);
        form.appendChild(input);
        form.appendChild(submit);

        const results = document.createElement('div');

        results.className = 'tech-map-search-results';
        results.hidden = true;

        form.addEventListener('submit', function (event) {

            event.preventDefault();

            runSearch(instance, input.value.trim());
        });

        input.addEventListener('keydown', function (event) {

            if (event.key === 'Escape') {

                hideSearchResults(instance);
                input.blur();
            }
        });

        wrap.appendChild(form);
        wrap.appendChild(results);
        controlsStart.appendChild(wrap);

        instance.searchForm = form;
        instance.searchInput = input;
        instance.searchResults = results;
    }

    function toggleFullscreen(instance) {

        const root = instance.element;
        const active = document.fullscreenElement ||
            document.webkitFullscreenElement;

        if (active === root) {

            const exit = document.exitFullscreen ||
                document.webkitExitFullscreen;

            if (exit) exit.call(document);

            return;
        }

        const request = root.requestFullscreen ||
            root.webkitRequestFullscreen;

        if (!request) return;

        const result = request.call(root);

        if (result && typeof result.catch === 'function') {

            result.catch(function () {
                Tech.notify(
                    'ورود به حالت تمام‌صفحه ممکن نیست.',
                    { type: 'warning' });
            });
        }
    }

    Tech.Map.get = function (target) {

        const element = target && target.nodeType === 1
            ? target
            : document.getElementById(target);

        return element && element._techMapInstance
            ? element._techMapInstance.api
            : null;
    };

    Tech.register('map', initMap);
})();


// ------------------------------------------------------------
// Boot
// ------------------------------------------------------------

if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', Tech.boot);
}
else {
    Tech.boot();
}
