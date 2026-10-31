// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

(function () {
    const app = window.WebQLCHCF = window.WebQLCHCF || {};
    const storageKey = "qlchcf.sidebar.collapsed";
    const root = document.documentElement;
    let sidebarAnimationTimer;
    root.dataset.pjaxEnabled = "true";

    function isMobileLayout() {
        return window.matchMedia("(max-width: 991.98px)").matches;
    }

    function runSidebarAnimation(action) {
        window.clearTimeout(sidebarAnimationTimer);
        root.classList.add("sidebar-animating");
        action();
        sidebarAnimationTimer = window.setTimeout(function () {
            root.classList.remove("sidebar-animating");
        }, 240);
    }

    function syncSidebarToggleState() {
        const collapsed = root.classList.contains("sidebar-collapsed");
        const toggle = document.getElementById("sidebarToggle");
        if (toggle) {
            toggle.setAttribute("aria-pressed", collapsed ? "true" : "false");
            toggle.setAttribute("aria-label", collapsed ? "Mở rộng sidebar" : "Thu gọn sidebar");
            toggle.title = collapsed ? "Mở rộng sidebar" : "Thu gọn sidebar";
        }
    }

    function setCollapsed(collapsed) {
        root.classList.toggle("sidebar-collapsed", collapsed);
        try {
            localStorage.setItem(storageKey, collapsed ? "true" : "false");
        } catch { }
        syncSidebarToggleState();
    }

    function closeMobileSidebar() {
        document.getElementById("app-sidebar")?.classList.remove("show");
    }

    function initSidebar() {
        const sidebar = document.getElementById("app-sidebar");
        const desktopToggle = document.getElementById("sidebarToggle");
        const mobileToggle = document.getElementById("sidebarMobileToggle");

        try {
            setCollapsed(localStorage.getItem(storageKey) === "true");
        } catch {
            setCollapsed(root.classList.contains("sidebar-collapsed"));
        }

        root.classList.remove("app-preload");
        root.classList.add("app-ready");

        if (!desktopToggle?.dataset.sidebarBound) {
            desktopToggle?.addEventListener("click", function () {
                if (isMobileLayout()) {
                    runSidebarAnimation(function () {
                        sidebar?.classList.toggle("show");
                    });
                    return;
                }

                runSidebarAnimation(function () {
                    setCollapsed(!root.classList.contains("sidebar-collapsed"));
                });
            });
            if (desktopToggle) {
                desktopToggle.dataset.sidebarBound = "true";
            }
        }

        if (!mobileToggle?.dataset.sidebarBound) {
            mobileToggle?.addEventListener("click", function (event) {
                event.stopPropagation();
                if (isMobileLayout()) {
                    runSidebarAnimation(function () {
                        sidebar?.classList.toggle("show");
                    });
                    return;
                }

                runSidebarAnimation(function () {
                    setCollapsed(!root.classList.contains("sidebar-collapsed"));
                });
            });
            if (mobileToggle) {
                mobileToggle.dataset.sidebarBound = "true";
            }
        }
    }

    function getPjaxContainer() {
        return document.querySelector("[data-pjax-container]");
    }

    function shouldSkipUrl(url) {
        const path = url.pathname.toLowerCase();
        const staticFile = /\.(css|js|json|png|jpg|jpeg|gif|svg|webp|ico|pdf|zip|rar|7z|docx?|xlsx?)$/i;

        return staticFile.test(path)
            || path.startsWith("/account")
            || path.includes("/logout")
            || path.includes("/checkout")
            || path.includes("/completetable")
            || path.includes("/delete");
    }

    function shouldHandleLink(link, event) {
        if (!link || event.defaultPrevented || event.button !== 0) {
            return false;
        }
        if (event.metaKey || event.ctrlKey || event.shiftKey || event.altKey) {
            return false;
        }
        if (link.dataset.noPjax !== undefined || link.target === "_blank" || link.hasAttribute("download")) {
            return false;
        }
        if (!link.closest("#app-sidebar, #app-topbar, #app-content")) {
            return false;
        }

        const href = link.getAttribute("href") || "";
        if (!href || href.startsWith("#") || href.startsWith("mailto:") || href.startsWith("tel:")) {
            return false;
        }

        let url;
        try {
            url = new URL(link.href, window.location.origin);
        } catch {
            return false;
        }

        if (url.origin !== window.location.origin || !["http:", "https:"].includes(url.protocol)) {
            return false;
        }
        if (shouldSkipUrl(url)) {
            return false;
        }
        if (url.pathname === window.location.pathname && url.search === window.location.search && url.hash) {
            return false;
        }

        return true;
    }

    function copySidebarActiveState(doc) {
        const currentMenu = document.querySelector("#app-sidebar .sidebar-menu");
        const nextMenu = doc.querySelector("#app-sidebar .sidebar-menu");
        if (currentMenu && nextMenu) {
            currentMenu.innerHTML = nextMenu.innerHTML;
        }
        syncSidebarToggleState();
    }

    function isLayoutScript(src) {
        const path = new URL(src, window.location.origin).pathname.toLowerCase();
        return path.includes("/lib/jquery/")
            || path.includes("/lib/bootstrap/")
            || path.endsWith("/js/site.js");
    }

    function isScriptAlreadyLoaded(src) {
        const path = new URL(src, window.location.origin).pathname.toLowerCase();
        return Array.from(document.scripts).some(function (script) {
            if (!script.src) {
                return false;
            }
            return new URL(script.src, window.location.origin).pathname.toLowerCase() === path;
        });
    }

    function loadExternalScript(src) {
        return new Promise(function (resolve, reject) {
            const script = document.createElement("script");
            script.src = src;
            script.dataset.pjaxLoaded = "true";
            script.onload = resolve;
            script.onerror = reject;
            document.body.appendChild(script);
        });
    }

    async function runPageScripts(doc) {
        const scripts = Array.from(doc.body.querySelectorAll("script"));
        for (const oldScript of scripts) {
            const src = oldScript.getAttribute("src");
            if (src) {
                const absoluteSrc = new URL(src, window.location.origin).href;
                if (isLayoutScript(absoluteSrc) || isScriptAlreadyLoaded(absoluteSrc)) {
                    continue;
                }
                await loadExternalScript(absoluteSrc);
                continue;
            }

            const code = oldScript.textContent || "";
            if (!code.trim()) {
                continue;
            }

            const script = document.createElement("script");
            script.textContent = code;
            document.body.appendChild(script);
            script.remove();
        }
    }

    async function renderPjaxHtml(html, url, options) {
        const doc = new DOMParser().parseFromString(html, "text/html");
        const nextContainer = doc.querySelector("[data-pjax-container]");
        const currentContainer = getPjaxContainer();

        if (!nextContainer || !currentContainer) {
            throw new Error("PJAX container was not found.");
        }

        currentContainer.replaceWith(document.importNode(nextContainer, true));
        document.title = doc.title || document.title;
        copySidebarActiveState(doc);

        if (options?.updateHistory !== false) {
            const state = { pjax: true, url: url };
            if (options?.replaceHistory) {
                window.history.replaceState(state, document.title, url);
            } else {
                window.history.pushState(state, document.title, url);
            }
        }

        await runPageScripts(doc);
        app.initPage(getPjaxContainer());
        root.dataset.pjaxCount = String((Number(root.dataset.pjaxCount || "0") || 0) + 1);

        const main = getPjaxContainer();
        if (main) {
            main.scrollTo?.({ top: 0 });
        }
        window.scrollTo({ top: 0 });
        closeMobileSidebar();
    }

    async function loadUrl(url, options) {
        root.classList.add("pjax-loading");
        try {
            const response = await fetch(url, {
                method: "GET",
                credentials: "same-origin",
                headers: {
                    "X-Requested-With": "XMLHttpRequest",
                    "X-PJAX": "true"
                }
            });

            if (!response.ok) {
                throw new Error("GET failed: " + response.status);
            }

            await renderPjaxHtml(await response.text(), response.url || url, options);
        } catch (error) {
            window.location.href = url;
        } finally {
            root.classList.remove("pjax-loading");
        }
    }

    function disableSubmitButtons(form, disabled) {
        form.querySelectorAll("button[type='submit'], input[type='submit']").forEach(function (button) {
            button.disabled = disabled;
        });
    }

    function showPjaxError(message) {
        const container = getPjaxContainer();
        if (!container) {
            return;
        }
        container.insertAdjacentHTML(
            "afterbegin",
            `<div class="alert alert-danger shadow-sm border-0 rounded-4 mb-4" role="alert">${message}</div>`
        );
    }

    async function submitPjaxForm(form) {
        if (form.dataset.pjaxSubmitting === "true") {
            return;
        }

        form.dataset.pjaxSubmitting = "true";
        disableSubmitButtons(form, true);
        root.classList.add("pjax-loading");

        try {
            const response = await fetch(form.action || window.location.href, {
                method: (form.method || "POST").toUpperCase(),
                credentials: "same-origin",
                body: new FormData(form),
                headers: {
                    "X-Requested-With": "XMLHttpRequest",
                    "X-PJAX": "true"
                }
            });

            if (!response.ok) {
                throw new Error("POST failed: " + response.status);
            }

            await renderPjaxHtml(await response.text(), response.url || window.location.href, {
                replaceHistory: response.redirected
            });
        } catch (error) {
            showPjaxError("Không thể xử lý thao tác. Trang sẽ tải lại để đảm bảo dữ liệu không bị sai.");
            window.setTimeout(function () {
                window.location.href = form.action || window.location.href;
            }, 700);
        } finally {
            root.classList.remove("pjax-loading");
            delete form.dataset.pjaxSubmitting;
            disableSubmitButtons(form, false);
        }
    }

    function submitGetForm(form) {
        const url = new URL(form.action || window.location.href, window.location.origin);
        const data = new FormData(form);
        url.search = new URLSearchParams(data).toString();
        loadUrl(url.href);
    }

    function initInvoicePaymentCountdowns(container) {
        const scope = container || document;
        scope.querySelectorAll("[data-expire-at]").forEach(function (element) {
            if (element.dataset.countdownBound === "true") {
                return;
            }

            const expiresAt = new Date(element.dataset.expireAt || "");
            if (Number.isNaN(expiresAt.getTime())) {
                return;
            }

            element.dataset.countdownBound = "true";
            const update = function () {
                const remainingMs = expiresAt.getTime() - Date.now();
                if (remainingMs <= 0) {
                    element.textContent = "QR hết hạn";
                    element.classList.remove("is-urgent");
                    element.classList.add("is-expired");
                    window.clearInterval(timer);
                    return;
                }

                const totalSeconds = Math.floor(remainingMs / 1000);
                const minutes = Math.floor(totalSeconds / 60);
                const seconds = totalSeconds % 60;
                element.textContent = "Còn " + String(minutes).padStart(2, "0") + ":" + String(seconds).padStart(2, "0");
                element.classList.toggle("is-urgent", remainingMs <= 120000);
            };

            const timer = window.setInterval(update, 1000);
            update();
        });
    }

    app.initPage = function (container) {
        const scope = container || document;

        if (window.jQuery && window.jQuery.validator && window.jQuery.validator.unobtrusive) {
            const $forms = window.jQuery(scope).find("form");
            $forms.each(function () {
                window.jQuery(this).removeData("validator");
                window.jQuery(this).removeData("unobtrusiveValidation");
            });
            window.jQuery.validator.unobtrusive.parse($forms);
        }

        if (window.bootstrap) {
            scope.querySelectorAll('[data-bs-toggle="tooltip"]').forEach(function (element) {
                if (!window.bootstrap.Tooltip.getInstance(element)) {
                    new window.bootstrap.Tooltip(element);
                }
            });
        }

        if (typeof app.initOrderCreate === "function") {
            app.initOrderCreate(scope);
        }

        if (typeof app.initRecipeEditor === "function") {
            app.initRecipeEditor(scope);
        }

        initInvoicePaymentCountdowns(scope);
    };

    app.pjax = {
        load: loadUrl,
        reload: function () {
            return loadUrl(window.location.href, { replaceHistory: true });
        }
    };

    document.addEventListener("click", function (event) {
        const link = event.target.closest?.("a[href]");
        if (!shouldHandleLink(link, event)) {
            return;
        }

        event.preventDefault();
        loadUrl(link.href);
    });

    document.addEventListener("submit", function (event) {
        const form = event.target.closest?.("form");
        if (!form || form.dataset.noPjax !== undefined) {
            return;
        }

        const method = (form.method || "GET").toUpperCase();
        if (method === "GET" && form.closest("#app-content")) {
            event.preventDefault();
            submitGetForm(form);
            return;
        }

        if (method === "POST" && form.dataset.pjaxForm === "true") {
            event.preventDefault();
            submitPjaxForm(form);
        }
    });

    document.addEventListener("click", function (event) {
        if (!isMobileLayout()) {
            return;
        }

        const target = event.target;
        if (
            target instanceof Element
            && !target.closest("#app-sidebar")
            && !target.closest("#sidebarMobileToggle")
        ) {
            closeMobileSidebar();
        }
    });

    window.addEventListener("resize", function () {
        if (!isMobileLayout()) {
            closeMobileSidebar();
        }
    });

    window.addEventListener("popstate", function () {
        loadUrl(window.location.href, {
            updateHistory: false
        });
    });

    document.addEventListener("DOMContentLoaded", function () {
        initSidebar();
        window.history.replaceState({ pjax: true, url: window.location.href }, document.title, window.location.href);
        app.initPage(getPjaxContainer());
    });
})();
