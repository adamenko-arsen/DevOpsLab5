const Auth = (() => {
    const STORAGE_KEY = 'lp_auth';

    function save(data) {
        localStorage.setItem(STORAGE_KEY, JSON.stringify(data));
    }

    function load() {
        try { return JSON.parse(localStorage.getItem(STORAGE_KEY)); }
        catch (e) { return null; }
    }

    return {
        getToken: function () { var d = load(); return d ? d.token : null; },
        getUser: function () { return load(); },
        isLoggedIn: function () { var d = load(); return !!(d && d.token); },
        getRole: function () { var d = load(); return d ? d.role : 'Guest'; },
        isAdmin: function () { var d = load(); return d && d.role === 'Admin'; },

        login: function (r) {
            console.log('[AUTH] Server response:', JSON.stringify(r));
            save({
                token: r.token || r.Token,
                username: r.username || r.Username,
                role: r.role || r.Role,
                userId: r.userId || r.UserId
            });
            var saved = load();
            console.log('[AUTH] Saved token:', saved && saved.token ? 'YES (' + saved.token.substring(0, 20) + '...)' : 'NO');
            console.log('[AUTH] Saved role:', saved ? saved.role : 'none');
        },

        logout: function () {
            localStorage.removeItem(STORAGE_KEY);
        }
    };
})();
