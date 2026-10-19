// Progressive-enhancement motion for the landing page.
// Loaded as an ES module from Login.razor via IJSRuntime import.
// Without this running, everything stays fully visible (the reveal styles only
// take effect once we add the `landing--js` flag) and the counters show their final values.

let observer = null;
let counterObserver = null;
let scrollHandler = null;
let headerEl = null;

const reduceMotion = () => window.matchMedia('(prefers-reduced-motion: reduce)').matches;

function setupReveals(root) {
    const targets = root.querySelectorAll('.reveal');
    if (targets.length === 0) {
        return;
    }

    if (observer) {
        observer.disconnect();
    }

    if (reduceMotion()) {
        targets.forEach(el => el.classList.add('is-visible'));
        return;
    }

    observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('is-visible');
                observer.unobserve(entry.target);
            }
        });
    }, { root: null, rootMargin: '0px 0px -10% 0px', threshold: 0.12 });

    targets.forEach(el => observer.observe(el));
}

// "12 418"-style grouping (thin no-break space), like the app's mileage figures.
const formatCount = (n) => Math.round(n).toLocaleString('en-US').replace(/,/g, ' ');

// Odometer-style count-up for [data-count] elements the first time they scroll into view.
function setupCounters(root) {
    const counters = root.querySelectorAll('[data-count]');
    if (counters.length === 0 || reduceMotion()) {
        return;
    }

    if (counterObserver) {
        counterObserver.disconnect();
    }

    const run = (el) => {
        const target = Number(el.dataset.count);
        if (!Number.isFinite(target) || el.dataset.counted === '1') {
            return;
        }
        el.dataset.counted = '1';

        const duration = 1600;
        const start = performance.now();
        const tick = (now) => {
            const t = Math.min(1, (now - start) / duration);
            const eased = 1 - Math.pow(1 - t, 3); // ease-out cubic — fast spin, slow settle
            el.textContent = formatCount(target * eased);
            if (t < 1) {
                requestAnimationFrame(tick);
            }
        };
        el.textContent = formatCount(0);
        requestAnimationFrame(tick);
    };

    counterObserver = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                run(entry.target);
                counterObserver.unobserve(entry.target);
            }
        });
    }, { root: null, threshold: 0.6 });

    counters.forEach(el => {
        if (el.dataset.counted !== '1') {
            counterObserver.observe(el);
        }
    });
}

function setupHeader(root) {
    headerEl = root.querySelector('.lp-header');
    if (!headerEl) {
        return;
    }

    const onScroll = () => {
        if (window.scrollY > 24) {
            headerEl.classList.add('lp-header--scrolled');
        } else {
            headerEl.classList.remove('lp-header--scrolled');
        }
    };

    if (scrollHandler) {
        window.removeEventListener('scroll', scrollHandler);
    }
    scrollHandler = onScroll;
    window.addEventListener('scroll', scrollHandler, { passive: true });
    onScroll();
}

// Blazor's router swallows in-page "#anchor" clicks, so wire smooth-scroll ourselves for [data-scroll-to] links.
function setupScrollLinks(root) {
    root.querySelectorAll('[data-scroll-to]').forEach(link => {
        if (link.dataset.bound === '1') {
            return;
        }
        link.dataset.bound = '1';
        link.addEventListener('click', (e) => {
            e.preventDefault();
            const target = root.querySelector('#' + link.dataset.scrollTo);
            if (target) {
                target.scrollIntoView({ behavior: reduceMotion() ? 'auto' : 'smooth', block: 'start' });
            }
        });
    });
}

// Idempotent: safe to call again after re-renders / once the anonymous content mounts.
export function init() {
    const root = document.querySelector('.lp');
    if (!root) {
        return;
    }

    root.classList.add('landing--js');
    setupReveals(root);
    setupCounters(root);
    setupHeader(root);
    setupScrollLinks(root);
}

export function dispose() {
    if (observer) {
        observer.disconnect();
        observer = null;
    }
    if (counterObserver) {
        counterObserver.disconnect();
        counterObserver = null;
    }
    if (scrollHandler) {
        window.removeEventListener('scroll', scrollHandler);
        scrollHandler = null;
    }
    headerEl = null;
}
