// Progressive-enhancement motion for the landing page.
// Loaded as an ES module from Login.razor via IJSRuntime import.
// Without this running, everything stays fully visible (the reveal styles only
// take effect once we add the `landing--js` flag).

let observer = null;
let scrollHandler = null;
let headerEl = null;

function setupReveals(root) {
    const targets = root.querySelectorAll('.reveal');
    if (targets.length === 0) {
        return;
    }

    if (observer) {
        observer.disconnect();
    }

    const reduceMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    if (reduceMotion) {
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

// Blazor's router swallows in-page "#anchor" clicks, so wire smooth-scroll ourselves.
function setupScrollCue(root) {
    const cue = root.querySelector('.lp-scroll-cue');
    if (!cue || cue.dataset.bound === '1') {
        return;
    }
    cue.dataset.bound = '1';
    cue.addEventListener('click', (e) => {
        e.preventDefault();
        const target = root.querySelector('#story');
        if (target) {
            target.scrollIntoView({ behavior: 'smooth', block: 'start' });
        }
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
    setupHeader(root);
    setupScrollCue(root);
}

export function dispose() {
    if (observer) {
        observer.disconnect();
        observer = null;
    }
    if (scrollHandler) {
        window.removeEventListener('scroll', scrollHandler);
        scrollHandler = null;
    }
    headerEl = null;
}
