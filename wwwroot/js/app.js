const App = (() => {
    const root = function () { return document.getElementById('app'); };
    var currentPage = 'catalog';
    var tagsCache = [];
    var favIdsCache = new Set();

    function toast(msg, type) {
        type = type || 'info';
        var c = document.querySelector('.lp-toast-container');
        if (!c) { c = document.createElement('div'); c.className = 'lp-toast-container'; document.body.appendChild(c); }
        var t = document.createElement('div');
        t.className = 'lp-toast ' + type;
        t.textContent = msg;
        c.appendChild(t);
        setTimeout(function () { t.remove(); if (!c.children.length) c.remove(); }, 3500);
    }

    function navigate(page, params) {
        currentPage = page;
        window.location.hash = params ? page + '/' + params : page;
        renderNavbar();
        switch (page) {
            case 'catalog': renderCatalog(); break;
            case 'detail': renderDetail(params); break;
            case 'login': renderLogin(); break;
            case 'register': renderRegister(); break;
            case 'dashboard': renderDashboard(); break;
            case 'stats': renderStats(); break;
            default: renderCatalog();
        }
    }

    function parseHash() {
        var h = window.location.hash.replace('#', '') || 'catalog';
        var parts = h.split('/');
        return { page: parts[0], params: parts.slice(1).join('/') };
    }

    function renderNavbar() {
        var user = Auth.getUser();
        var nav = document.getElementById('navbar-links');
        if (!nav) return;
        var links = '';
        links += '<button class="lp-nav-link ' + (currentPage === 'catalog' ? 'active' : '') + '" onclick="App.navigate(\'catalog\')">Catalog</button>';
        links += '<button class="lp-nav-link ' + (currentPage === 'stats' ? 'active' : '') + '" onclick="App.navigate(\'stats\')">Statistics</button>';
        if (user && user.token) {
            links += '<button class="lp-nav-link ' + (currentPage === 'dashboard' ? 'active' : '') + '" onclick="App.navigate(\'dashboard\')">Dashboard</button>';
            links += '<span class="lp-nav-link" style="cursor:default;color:var(--lp-accent);">' + (user.role === 'Admin' ? '\u2699 ' : '') + user.username + '</span>';
            links += '<button class="lp-nav-link" onclick="Auth.logout(); App.navigate(\'catalog\'); App.toast(\'Logged out\',\'success\');">Logout</button>';
        } else {
            links += '<button class="lp-nav-link ' + (currentPage === 'login' ? 'active' : '') + '" onclick="App.navigate(\'login\')">Login</button>';
            links += '<button class="lp-nav-link accent" onclick="App.navigate(\'register\')">Sign Up</button>';
        }
        nav.innerHTML = links;
    }

    var catalogState = { search: '', language: '', tagId: '', sort: 'newest', page: 1 };

    async function renderCatalog() {
        root().innerHTML = '<div class="lp-spinner"></div>';
        try {
            if (!tagsCache.length) tagsCache = await Api.get('/tags');
            var languages = await Api.get('/libraries/languages');
            if (Auth.isLoggedIn()) {
                try { favIdsCache = new Set(await Api.get('/favorites/ids')); } catch (e) { console.log('[CATALOG] favorites/ids failed:', e.message); }
            }
            var langOptions = '<option value="">All Languages</option>';
            languages.forEach(function (l) {
                langOptions += '<option value="' + l + '"' + (catalogState.language === l ? ' selected' : '') + '>' + l + '</option>';
            });
            var tagOptions = '<option value="">All Tags</option>';
            tagsCache.forEach(function (t) {
                tagOptions += '<option value="' + t.id + '"' + (catalogState.tagId == t.id ? ' selected' : '') + '>' + t.name + '</option>';
            });
            root().innerHTML =
                '<div style="display:flex;justify-content:space-between;align-items:center;margin-bottom:1rem;flex-wrap:wrap;gap:.5rem;">' +
                '<h1 style="margin:0;font-size:1.5rem;">Library Catalog</h1>' +
                (Auth.isLoggedIn() ? '<button class="btn-lp" onclick="App.showAddModal()">+ Add Library</button>' : '') +
                '</div>' +
                '<div class="lp-filters">' +
                '<input class="lp-search-input" id="f-search" placeholder="Search libraries..." value="' + catalogState.search + '" oninput="App.onFilter()">' +
                '<select class="lp-select" id="f-lang" onchange="App.onFilter()">' + langOptions + '</select>' +
                '<select class="lp-select" id="f-tag" onchange="App.onFilter()">' + tagOptions + '</select>' +
                '<select class="lp-select" id="f-sort" onchange="App.onFilter()">' +
                '<option value="newest"' + (catalogState.sort === 'newest' ? ' selected' : '') + '>Newest</option>' +
                '<option value="stars"' + (catalogState.sort === 'stars' ? ' selected' : '') + '>Most Stars</option>' +
                '<option value="rating"' + (catalogState.sort === 'rating' ? ' selected' : '') + '>Top Rated</option>' +
                '</select>' +
                '</div>' +
                '<div id="catalog-grid" class="lp-grid"></div>' +
                '<div id="catalog-pagination" class="lp-pagination"></div>';
            await fetchCatalog();
        } catch (e) {
            root().innerHTML = '<p style="color:var(--lp-red)">Error: ' + e.message + '</p>';
        }
    }

    var filterTimeout;
    function onFilter() {
        clearTimeout(filterTimeout);
        filterTimeout = setTimeout(function () {
            catalogState.search = document.getElementById('f-search').value;
            catalogState.language = document.getElementById('f-lang').value;
            catalogState.tagId = document.getElementById('f-tag').value;
            catalogState.sort = document.getElementById('f-sort').value;
            catalogState.page = 1;
            fetchCatalog();
        }, 300);
    }

    async function fetchCatalog() {
        var grid = document.getElementById('catalog-grid');
        var pag = document.getElementById('catalog-pagination');
        if (!grid) return;
        grid.innerHTML = '<div class="lp-spinner" style="grid-column:1/-1"></div>';
        var params = new URLSearchParams();
        if (catalogState.search) params.set('search', catalogState.search);
        if (catalogState.language) params.set('language', catalogState.language);
        if (catalogState.tagId) params.set('tagId', catalogState.tagId);
        params.set('sort', catalogState.sort);
        params.set('page', catalogState.page);
        params.set('pageSize', 12);
        try {
            var res = await Api.get('/libraries?' + params.toString());
            if (!res.data.length) {
                grid.innerHTML = '<p style="grid-column:1/-1;text-align:center;color:var(--lp-muted);">No libraries found.</p>';
                pag.innerHTML = '';
                return;
            }
            grid.innerHTML = res.data.map(function (lib) { return renderLibCard(lib); }).join('');
            var totalPages = Math.ceil(res.total / res.pageSize);
            if (totalPages > 1) {
                var pages = '';
                for (var p = 1; p <= totalPages; p++) {
                    pages += '<button class="lp-page-btn ' + (p === res.page ? 'active' : '') + '" onclick="App.goPage(' + p + ')">' + p + '</button>';
                }
                pag.innerHTML = pages;
            } else { pag.innerHTML = ''; }
        } catch (e) {
            grid.innerHTML = '<p style="color:var(--lp-red)">' + e.message + '</p>';
        }
    }

    function goPage(p) { catalogState.page = p; fetchCatalog(); }

    function renderLibCard(lib) {
        var rating = lib.averageRating || 0;
        var full = Math.round(rating);
        var stars = '\u2605'.repeat(full) + '\u2606'.repeat(5 - full);
        var isFav = favIdsCache.has(lib.id);
        var favBtn = Auth.isLoggedIn()
            ? '<button class="btn-fav" onclick="event.stopPropagation(); App.toggleFav(' + lib.id + ')" title="' + (isFav ? 'Remove from favorites' : 'Add to favorites') + '">' + (isFav ? '\u2764\uFE0F' : '\uD83E\uDD0D') + '</button>'
            : '';
        var desc = lib.description ? (lib.description.length > 100 ? lib.description.substring(0, 100) + '...' : lib.description) : '';
        var tags = '';
        if (lib.tags) lib.tags.forEach(function (t) { tags += '<span class="lp-badge lp-badge-tag">' + t.name + '</span>'; });
        return '<div class="lp-card" style="cursor:pointer" onclick="App.navigate(\'detail\',\'' + lib.id + '\')">' +
            '<div style="display:flex;justify-content:space-between;align-items:flex-start;">' +
            '<div class="lp-card-title">' + lib.name + ' <span style="font-weight:400;font-size:.8rem;color:var(--lp-muted)">v' + lib.version + '</span></div>' +
            favBtn +
            '</div>' +
            '<p style="font-size:.8rem;color:var(--lp-muted);margin:.3rem 0 .6rem;">' + desc + '</p>' +
            '<div style="display:flex;flex-wrap:wrap;gap:.3rem;margin-bottom:.5rem;">' +
            '<span class="lp-badge lp-badge-lang">' + lib.language + '</span>' +
            '<span class="lp-badge lp-badge-license">' + lib.licenseType + '</span>' +
            tags +
            '</div>' +
            '<div style="display:flex;justify-content:space-between;align-items:center;">' +
            '<span class="lp-stars" title="Rating: ' + rating + '/5">' + stars + ' <small>(' + lib.reviewCount + ')</small></span>' +
            (lib.gitHubStars != null ? '<span class="lp-meta">\u2B50 ' + lib.gitHubStars.toLocaleString() + ' GitHub stars</span>' : '') +
            '</div>' +
            '</div>';
    }

    async function renderDetail(id) {
        root().innerHTML = '<div class="lp-spinner"></div>';
        try {
            var lib = await Api.get('/libraries/' + id);
            var reviews = await Api.get('/libraries/' + id + '/reviews');
            var user = Auth.getUser();
            var isAdmin = Auth.isAdmin();
            var isOwner = user && lib.createdByUsername === user.username;
            var rating = lib.averageRating || 0;
            var full = Math.round(rating);
            var stars = '\u2605'.repeat(full) + '\u2606'.repeat(5 - full);
            var tagsHtml = '';
            if (lib.tags) lib.tags.forEach(function (t) { tagsHtml += '<span class="lp-badge lp-badge-tag">' + t.name + '</span>'; });
            var reviewsHtml = '';
            if (reviews.length) {
                reviews.forEach(function (r) {
                    var rStars = '\u2605'.repeat(r.rating) + '\u2606'.repeat(5 - r.rating);
                    var canDel = isAdmin || (user && r.username === user.username);
                    reviewsHtml +=
                        '<div class="lp-review"><div class="lp-review-header"><div><strong>' + r.username + '</strong> <span class="lp-review-stars">' + rStars + '</span></div>' +
                        '<div style="display:flex;align-items:center;gap:.5rem;"><span class="lp-meta">' + new Date(r.createdAt).toLocaleDateString() + '</span>' +
                        (canDel ? '<button class="btn-danger" onclick="App.deleteReview(' + r.id + ', ' + lib.id + ')">\u00D7</button>' : '') +
                        '</div></div><p style="margin:.4rem 0 0;font-size:.9rem;">' + r.comment + '</p></div>';
                });
            } else { reviewsHtml = '<p class="lp-meta">No reviews yet. Be the first!</p>'; }
            var githubHtml = '';
            if (lib.gitHubStars != null) {
                githubHtml = '<div class="lp-card" style="margin-bottom:1.5rem;"><h3 style="margin-top:0;font-size:1rem;">GitHub Statistics</h3>' +
                    '<div class="lp-github-stats">' +
                    '<div class="lp-github-stat"><span class="lp-github-stat-val" style="color:var(--lp-orange)">\u2B50 ' + lib.gitHubStars.toLocaleString() + '</span><span class="lp-github-stat-label">Stars</span></div>' +
                    '<div class="lp-github-stat"><span class="lp-github-stat-val" style="color:var(--lp-red)">\uD83D\uDD34 ' + (lib.gitHubOpenIssues || 0).toLocaleString() + '</span><span class="lp-github-stat-label">Open Issues</span></div>' +
                    '<div class="lp-github-stat"><span class="lp-github-stat-val" style="color:var(--lp-green)">\uD83D\uDCC5 ' + (lib.gitHubLastUpdated ? new Date(lib.gitHubLastUpdated).toLocaleDateString() : 'N/A') + '</span><span class="lp-github-stat-label">Last Updated</span></div>' +
                    '</div>' +
                    (isAdmin ? '<button class="btn-lp-outline" style="margin-top:.75rem;font-size:.75rem" onclick="App.refreshGitHub(' + lib.id + ')">\uD83D\uDD04 Refresh GitHub Data</button>' : '') +
                    '</div>';
            }
            root().innerHTML =
                '<button class="lp-nav-link" onclick="App.navigate(\'catalog\')" style="margin-bottom:1rem;">\u2190 Back to Catalog</button>' +
                '<div class="lp-detail-header"><div>' +
                '<h1>' + lib.name + ' <span style="font-weight:400;font-size:1rem;color:var(--lp-muted)">v' + lib.version + '</span></h1>' +
                '<div style="display:flex;flex-wrap:wrap;gap:.4rem;margin-top:.5rem;">' +
                '<span class="lp-badge lp-badge-lang">' + lib.language + '</span><span class="lp-badge lp-badge-license">' + lib.licenseType + '</span>' + tagsHtml +
                '</div></div>' +
                '<div style="display:flex;gap:.5rem;">' +
                ((isAdmin || isOwner) ? '<button class="btn-lp-outline" onclick="App.showEditModal(' + lib.id + ')">Edit</button>' : '') +
                (isAdmin ? '<button class="btn-danger" onclick="App.deleteLib(' + lib.id + ')">Delete</button>' : '') +
                '</div></div>' +
                '<div class="lp-card" style="margin-bottom:1.5rem;"><p style="margin:0;">' + (lib.description || 'No description.') + '</p>' +
                (lib.repositoryLink ? '<p style="margin:.5rem 0 0"><a href="' + lib.repositoryLink + '" target="_blank" style="color:var(--lp-accent)">View Repository \u2192</a></p>' : '') +
                '</div>' + githubHtml +
                '<div style="display:flex;justify-content:space-between;align-items:center;margin-bottom:1rem;">' +
                '<h3 style="margin:0;">Reviews <span class="lp-stars">' + stars + '</span> <small style="color:var(--lp-muted)">(' + lib.reviewCount + ')</small></h3>' +
                (user && user.token ? '<button class="btn-lp" onclick="App.showReviewModal(' + lib.id + ')">Write Review</button>' : '<span class="lp-meta">Login to leave a review</span>') +
                '</div><div id="reviews-list">' + reviewsHtml + '</div>';
        } catch (e) {
            root().innerHTML = '<p style="color:var(--lp-red)">Error loading library: ' + e.message + '</p>';
        }
    }

    function renderLogin() {
        root().innerHTML =
            '<div class="lp-auth-container">' +
            '<div style="text-align:center;margin-bottom:1.5rem;">' +
            '<div style="font-size:2.5rem;margin-bottom:.5rem;">\uD83D\uDD10</div>' +
            '<h2 style="margin:0;">Welcome Back</h2>' +
            '<p class="lp-meta" style="margin-top:.4rem;">Sign in to your account</p></div>' +
            '<div class="lp-card">' +
            '<div class="lp-form-group"><label for="login-email">Email</label><input type="email" id="login-email" placeholder="you@example.com"></div>' +
            '<div class="lp-form-group"><label for="login-pass">Password</label><input type="password" id="login-pass" placeholder="\u2022\u2022\u2022\u2022\u2022\u2022\u2022\u2022" onkeydown="if(event.key===\'Enter\') App.doLogin()"></div>' +
            '<button class="btn-lp" style="width:100%;margin-top:1.25rem;padding:.7rem;" onclick="App.doLogin()">Login</button>' +
            '<p style="text-align:center;margin-top:1rem;font-size:.85rem;color:var(--lp-muted);">Don\'t have an account? <a href="#" onclick="event.preventDefault(); App.navigate(\'register\')" style="color:var(--lp-accent)">Sign up</a></p>' +
            '</div></div>';
    }

    function renderRegister() {
        root().innerHTML =
            '<div class="lp-auth-container">' +
            '<div style="text-align:center;margin-bottom:1.5rem;">' +
            '<div style="font-size:2.5rem;margin-bottom:.5rem;">\u2728</div>' +
            '<h2 style="margin:0;">Create Account</h2>' +
            '<p class="lp-meta" style="margin-top:.4rem;">Join the developer community</p></div>' +
            '<div class="lp-card">' +
            '<div class="lp-form-group"><label for="reg-name">Username</label><input type="text" id="reg-name" placeholder="johndoe"></div>' +
            '<div class="lp-form-group"><label for="reg-email">Email</label><input type="email" id="reg-email" placeholder="you@example.com"></div>' +
            '<div class="lp-form-group"><label for="reg-pass">Password</label><input type="password" id="reg-pass" placeholder="\u2022\u2022\u2022\u2022\u2022\u2022\u2022\u2022" onkeydown="if(event.key===\'Enter\') App.doRegister()"></div>' +
            '<button class="btn-lp" style="width:100%;margin-top:1.25rem;padding:.7rem;" onclick="App.doRegister()">Sign Up</button>' +
            '<p style="text-align:center;margin-top:1rem;font-size:.85rem;color:var(--lp-muted);">Already have an account? <a href="#" onclick="event.preventDefault(); App.navigate(\'login\')" style="color:var(--lp-accent)">Login</a></p>' +
            '</div></div>';
    }

    async function doLogin() {
        var email = document.getElementById('login-email').value;
        var password = document.getElementById('login-pass').value;
        try {
            var res = await Api.post('/auth/login', { email: email, password: password });
            Auth.login(res);
            toast('Logged in successfully!', 'success');
            navigate('catalog');
        } catch (e) { toast(e.message || 'Invalid credentials', 'error'); }
    }

    async function doRegister() {
        var username = document.getElementById('reg-name').value;
        var email = document.getElementById('reg-email').value;
        var password = document.getElementById('reg-pass').value;
        try {
            var res = await Api.post('/auth/register', { username: username, email: email, password: password });
            Auth.login(res);
            toast('Account created!', 'success');
            navigate('catalog');
        } catch (e) { toast(e.message, 'error'); }
    }

    async function renderDashboard() {
        if (!Auth.isLoggedIn()) return navigate('login');
        root().innerHTML = '<div class="lp-spinner"></div>';
        try {
            var user = Auth.getUser();
            var favs = await Api.get('/favorites');
            root().innerHTML =
                '<h1 style="margin-bottom:.25rem;">My Dashboard</h1>' +
                '<p class="lp-meta" style="margin-bottom:1.5rem;">Welcome, <strong>' + user.username + '</strong> (' + user.role + ')</p>' +
                '<h3>My Favorites (' + favs.length + ')</h3>' +
                (favs.length
                    ? '<div class="lp-grid">' + favs.map(function (lib) { return renderLibCard(lib); }).join('') + '</div>'
                    : '<p class="lp-meta">No favorites yet. Browse the catalog and add some!</p>');
        } catch (e) {
            console.error('[DASHBOARD] Error:', e.message);
            root().innerHTML = '<h1>My Dashboard</h1><p style="color:var(--lp-red)">Error loading dashboard: ' + e.message + '</p>';
        }
    }

    async function renderStats() {
        root().innerHTML = '<div class="lp-spinner"></div>';
        try {
            var stats = await Api.get('/reports/stats');
            console.log('[STATS] Data received:', JSON.stringify(stats));
            var user = Auth.getUser();
            var isAdm = user && user.role === 'Admin';
            console.log('[STATS] isAdmin:', isAdm, 'role:', user ? user.role : 'no user');
            root().innerHTML =
                '<h1 style="margin-bottom:1rem;">Platform Statistics</h1>' +
                '<div style="display:grid;grid-template-columns:repeat(auto-fit,minmax(200px,1fr));gap:1rem;margin-bottom:2rem;">' +
                '<div class="lp-stat-box"><div class="lp-stat-number">' + stats.totalLibraries + '</div><div class="lp-stat-label">Libraries</div></div>' +
                '<div class="lp-stat-box"><div class="lp-stat-number">' + stats.totalUsers + '</div><div class="lp-stat-label">Users</div></div>' +
                '<div class="lp-stat-box"><div class="lp-stat-number">' + stats.totalReviews + '</div><div class="lp-stat-label">Reviews</div></div>' +
                '</div>' +
                '<div class="lp-chart-wrap"><h3 style="margin-top:0;">Libraries by Programming Language</h3><canvas id="lang-chart" width="1200" height="400" style="display:block;max-width:100%;"></canvas></div>' +
                (isAdm
                    ? '<div style="display:flex;gap:1rem;flex-wrap:wrap;margin-top:1.5rem;">' +
                    '<button class="btn-lp" onclick="App.exportExcel()">\uD83D\uDCE5 Export Excel</button>' +
                    '<button class="btn-lp-outline" onclick="App.exportPdf()">\uD83D\uDCC4 Export PDF</button></div>'
                    : '');
            renderLanguageChart(stats.languageDistribution);
        } catch (e) {
            console.error('[STATS] Error:', e.message);
            root().innerHTML = '<h1>Platform Statistics</h1><p style="color:var(--lp-red)">Error loading statistics: ' + e.message + '</p>';
        }
    }

    function renderLanguageChart(data) {
        var ctx = document.getElementById('lang-chart');
        if (!ctx) { console.error('[CHART] Canvas not found'); return; }
        if (!window.Chart) { console.error('[CHART] Chart.js not loaded'); return; }
        ctx.style.maxHeight = '300px';
        var colors = ['#6c5ce7', '#00b894', '#fdcb6e', '#e17055', '#0984e3', '#a29bfe', '#fab1a0', '#81ecec', '#ffeaa7', '#dfe6e9'];
        new Chart(ctx, {
            type: 'bar',
            data: {
                labels: data.map(function (d) { return d.language; }),
                datasets: [{
                    label: 'Number of Libraries',
                    data: data.map(function (d) { return d.count; }),
                    backgroundColor: data.map(function (_, i) { return colors[i % colors.length]; }),
                    borderRadius: 6,
                    barThickness: 40
                }]
            },
            options: {
                responsive: false,
                plugins: { legend: { display: false } },
                scales: {
                    y: { beginAtZero: true, ticks: { color: '#8b90a5', stepSize: 1 }, grid: { color: 'rgba(46,51,72,.5)' } },
                    x: { ticks: { color: '#8b90a5' }, grid: { display: false } }
                }
            }
        });
    }

    async function exportExcel() {
        try { await Api.download('/reports/export/excel', 'Libraries.xlsx'); toast('Excel downloaded!', 'success'); }
        catch (e) { toast(e.message, 'error'); }
    }

    async function exportPdf() {
        try { await Api.download('/reports/export/pdf', 'Libraries.pdf'); toast('PDF downloaded!', 'success'); }
        catch (e) { toast(e.message, 'error'); }
    }

    async function showAddModal() {
        if (!tagsCache.length) tagsCache = await Api.get('/tags');
        var languages = await Api.get('/libraries/languages');
        showLibModal('Add New Library', null, languages);
    }

    async function showEditModal(id) {
        var lib = await Api.get('/libraries/' + id);
        if (!tagsCache.length) tagsCache = await Api.get('/tags');
        var languages = await Api.get('/libraries/languages');
        showLibModal('Edit Library', lib, languages);
    }

    function showLibModal(title, lib, languages) {
        var overlay = document.createElement('div');
        overlay.className = 'lp-modal-overlay';
        overlay.onclick = function (e) { if (e.target === overlay) overlay.remove(); };
        var selectedTags = lib ? lib.tags.map(function (t) { return t.id; }) : [];
        var allLangs = Array.from(new Set([].concat(languages, ['JavaScript', 'Python', 'C#', 'Java', 'TypeScript', 'Go', 'Rust', 'Ruby', 'PHP', 'C++']))).sort();
        var langOpts = '<option value="">Select language...</option>';
        allLangs.forEach(function (l) { langOpts += '<option value="' + l + '"' + (lib && lib.language === l ? ' selected' : '') + '>' + l + '</option>'; });
        var tagChecks = '';
        tagsCache.forEach(function (t) { tagChecks += '<label><input type="checkbox" name="m-tags" value="' + t.id + '"' + (selectedTags.includes(t.id) ? ' checked' : '') + '>' + t.name + '</label>'; });
        overlay.innerHTML =
            '<div class="lp-modal"><h2>' + title + '</h2>' +
            '<label>Name</label><input id="m-name" value="' + (lib ? lib.name : '') + '">' +
            '<label>Version</label><input id="m-version" value="' + (lib ? lib.version : '') + '">' +
            '<label>Programming Language</label><select id="m-lang">' + langOpts + '</select>' +
            '<label>License Type</label><input id="m-license" value="' + (lib ? lib.licenseType : '') + '" placeholder="MIT, Apache-2.0, BSD-3...">' +
            '<label>Repository Link</label><input id="m-repo" value="' + (lib ? lib.repositoryLink : '') + '" placeholder="https://github.com/owner/repo">' +
            '<label>Description</label><textarea id="m-desc">' + (lib ? lib.description : '') + '</textarea>' +
            '<label>Tags</label><div class="tag-checkboxes">' + tagChecks + '</div>' +
            '<div class="lp-modal-actions"><button class="btn-lp-outline" onclick="this.closest(\'.lp-modal-overlay\').remove()">Cancel</button>' +
            '<button class="btn-lp" id="m-submit">' + (lib ? 'Save Changes' : 'Add Library') + '</button></div></div>';
        document.body.appendChild(overlay);
        document.getElementById('m-submit').onclick = async function () {
            var checkedTags = document.querySelectorAll('input[name="m-tags"]:checked');
            var tagIds = []; checkedTags.forEach(function (cb) { tagIds.push(parseInt(cb.value)); });
            var dto = {
                name: document.getElementById('m-name').value,
                version: document.getElementById('m-version').value,
                language: document.getElementById('m-lang').value,
                licenseType: document.getElementById('m-license').value,
                repositoryLink: document.getElementById('m-repo').value,
                description: document.getElementById('m-desc').value,
                tagIds: tagIds
            };
            try {
                if (lib) { await Api.put('/libraries/' + lib.id, dto); toast('Library updated!', 'success'); navigate('detail', lib.id); }
                else { var r = await Api.post('/libraries', dto); toast('Library added!', 'success'); navigate('detail', r.id); }
                overlay.remove();
            } catch (e) { toast(e.message, 'error'); }
        };
    }

    function showReviewModal(libId) {
        var overlay = document.createElement('div');
        overlay.className = 'lp-modal-overlay';
        overlay.onclick = function (e) { if (e.target === overlay) overlay.remove(); };
        var starSpans = '';
        for (var i = 1; i <= 5; i++) { starSpans += '<span data-val="' + i + '" style="color:var(--lp-border)" onmouseenter="App.hoverStar(' + i + ')" onclick="App.selectStar(' + i + ')">\u2605</span>'; }
        overlay.innerHTML =
            '<div class="lp-modal" style="max-width:420px;"><h2>Write a Review</h2>' +
            '<label>Rating</label><div id="star-select" style="font-size:1.6rem;cursor:pointer;margin:.3rem 0 .5rem;">' + starSpans + '</div>' +
            '<input type="hidden" id="r-rating" value="0">' +
            '<label>Comment</label><textarea id="r-comment" placeholder="Share your experience..."></textarea>' +
            '<div class="lp-modal-actions"><button class="btn-lp-outline" onclick="this.closest(\'.lp-modal-overlay\').remove()">Cancel</button>' +
            '<button class="btn-lp" id="r-submit">Submit Review</button></div></div>';
        document.body.appendChild(overlay);
        document.getElementById('r-submit').onclick = async function () {
            var rating = parseInt(document.getElementById('r-rating').value);
            var comment = document.getElementById('r-comment').value;
            if (!rating) return toast('Please select a rating', 'error');
            try { await Api.post('/libraries/' + libId + '/reviews', { rating: rating, comment: comment }); toast('Review submitted!', 'success'); overlay.remove(); renderDetail(libId); }
            catch (e) { toast(e.message, 'error'); }
        };
    }

    function hoverStar(val) {
        var spans = document.querySelectorAll('#star-select span');
        spans.forEach(function (s) { s.style.color = parseInt(s.dataset.val) <= val ? 'var(--lp-orange)' : 'var(--lp-border)'; });
    }
    function selectStar(val) { document.getElementById('r-rating').value = val; hoverStar(val); }

    async function toggleFav(libId) {
        if (!Auth.isLoggedIn()) return navigate('login');
        try {
            if (favIdsCache.has(libId)) { await Api.del('/favorites/' + libId); favIdsCache.delete(libId); toast('Removed from favorites', 'success'); }
            else { await Api.post('/favorites/' + libId); favIdsCache.add(libId); toast('Added to favorites!', 'success'); }
            fetchCatalog();
        } catch (e) { toast(e.message, 'error'); }
    }

    async function deleteLib(id) {
        if (!confirm('Delete this library permanently?')) return;
        try { await Api.del('/libraries/' + id); toast('Library deleted', 'success'); navigate('catalog'); }
        catch (e) { toast(e.message, 'error'); }
    }

    async function deleteReview(reviewId, libId) {
        if (!confirm('Delete this review?')) return;
        try { await Api.del('/reviews/' + reviewId); toast('Review deleted', 'success'); renderDetail(libId); }
        catch (e) { toast(e.message, 'error'); }
    }

    async function refreshGitHub(id) {
        try { var info = await Api.post('/libraries/' + id + '/refresh-github'); toast('Updated! \u2B50 ' + info.stars + ' stars', 'success'); renderDetail(id); }
        catch (e) { toast(e.message, 'error'); }
    }

    function init() {
        window.addEventListener('hashchange', function () { var h = parseHash(); navigate(h.page, h.params); });
        var h = parseHash();
        navigate(h.page, h.params);
    }

    return {
        init: init, navigate: navigate, toast: toast, onFilter: onFilter, goPage: goPage,
        showAddModal: showAddModal, showEditModal: showEditModal, showReviewModal: showReviewModal,
        hoverStar: hoverStar, selectStar: selectStar, doLogin: doLogin, doRegister: doRegister,
        toggleFav: toggleFav, deleteLib: deleteLib, deleteReview: deleteReview, refreshGitHub: refreshGitHub,
        exportExcel: exportExcel, exportPdf: exportPdf
    };
})();

document.addEventListener('DOMContentLoaded', App.init);