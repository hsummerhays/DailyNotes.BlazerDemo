function applyTheme(theme) {
    const html = document.documentElement;
    let effectiveTheme = theme;

    if (theme === 'device') {
        const isDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
        effectiveTheme = isDark ? 'dark' : 'light';
    }

    html.setAttribute('data-theme', effectiveTheme);
    localStorage.setItem('theme-preference', theme);
}

function getPreferredTheme() {
    return localStorage.getItem('theme-preference') || 'device';
}

// Initialize on load
(function() {
    const preferred = getPreferredTheme();
    applyTheme(preferred);
    
    // Listen for system theme changes
    window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', e => {
        if (getPreferredTheme() === 'device') {
            applyTheme('device');
        }
    });
})();

window.themeManager = {
    setTheme: applyTheme,
    getTheme: getPreferredTheme
};

