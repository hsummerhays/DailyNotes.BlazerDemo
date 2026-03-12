function setTheme(theme) {
    const html = document.documentElement;
    localStorage.setItem('theme-preference', theme);
    
    let activeTheme = theme;
    if (theme === 'device') {
        const isDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
        activeTheme = isDark ? 'dark' : 'light';
        html.setAttribute('data-theme-preference', 'device');
    } else {
        html.removeAttribute('data-theme-preference');
    }
    
    html.setAttribute('data-theme', activeTheme);
}

function getPreferredTheme() {
    return localStorage.getItem('theme-preference') || 'device';
}

// Initialize and listen for system changes
(function() {
    setTheme(getPreferredTheme());
    window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', () => {
        if (getPreferredTheme() === 'device') setTheme('device');
    });
})();

window.themeManager = {
    setTheme: setTheme,
    getTheme: getPreferredTheme
};
