const Api = (() => {
    const BASE = '/api';

    function headers(json = true) {
        const h = {};
        if (json) h['Content-Type'] = 'application/json';
        const token = Auth.getToken();
        if (token) h['Authorization'] = 'Bearer ' + token;
        return h;
    }

    async function request(method, path, body) {
        const opts = { method, headers: headers() };
        if (body) opts.body = JSON.stringify(body);

        console.log('[API]', method, path, Auth.getToken() ? '(with token)' : '(no token)');

        const res = await fetch(BASE + path, opts);

        console.log('[API]', method, path, '->', res.status);

        if (res.status === 401) {
            throw new Error('Unauthorized');
        }
        if (res.status === 403) {
            throw new Error('Forbidden - access denied');
        }
        if (res.status === 204) return null;
        const ct = res.headers.get('content-type') || '';
        if (ct.includes('json')) {
            const data = await res.json();
            if (!res.ok) throw new Error(data.error || data.title || 'Request failed');
            return data;
        }
        if (!res.ok) {
            const text = await res.text();
            console.error('[API] Error body:', text);
            throw new Error('Request failed: ' + res.status);
        }
        return res;
    }

    return {
        get: function (path) { return request('GET', path); },
        post: function (path, body) { return request('POST', path, body); },
        put: function (path, body) { return request('PUT', path, body); },
        del: function (path) { return request('DELETE', path); },
        download: async function (path, filename) {
            var res = await fetch(BASE + path, { headers: headers(false) });
            if (!res.ok) throw new Error('Download failed: ' + res.status);
            var blob = await res.blob();
            var url = URL.createObjectURL(blob);
            var a = document.createElement('a');
            a.href = url;
            a.download = filename;
            document.body.appendChild(a);
            a.click();
            document.body.removeChild(a);
            URL.revokeObjectURL(url);
        }
    };
})();
