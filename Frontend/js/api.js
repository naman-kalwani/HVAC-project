/* HVAC Connect API client. Configure window.HVAC_API_URL before this file when hosted separately. */
(() => {
    const base = (window.HVAC_API_URL || "http://localhost:5000/api").replace(/\/$/, "");
    const request = async (path, options = {}) => {
        const response = await fetch(`${base}${path}`, {
            ...options,
            headers: { "Content-Type": "application/json", ...(options.headers || {}) }
        });
        if (!response.ok) {
            const body = await response.json().catch(() => ({}));
            const validationMessage = body.errors ? Object.values(body.errors).flat().join(" ") : "";
            const message = body.detail || body.message || validationMessage || (response.status === 401 ? "Email or password is incorrect." : `API request failed (${response.status})`);
            throw new Error(message);
        }
        return response.status === 204 ? null : response.json();
    };
    window.HvacApi = {
        baseUrl: base,
        request,
        login: (email, password) => request("/auth/login", { method: "POST", body: JSON.stringify({ email, password }) }),
        signup: (name, email, password) => request("/auth/signup", { method: "POST", body: JSON.stringify({ name, email, password }) }),
        me: id => request(`/auth/me/${id}`),
        dashboard: () => request("/dashboard/summary"),
        vendors: () => request("/vendors"), customers: () => request("/customers"),
        locations: () => request("/locations"), assets: () => request("/assets"),
        tickets: () => request("/tickets"), technicians: () => request("/technicians"),
        imports: () => request("/imports"), duplicates: () => request("/duplicates"), audit: () => request("/audit"),
        create: (resource, payload) => request(`/${resource}`, { method: "POST", body: JSON.stringify(payload) }),
        updateTicket: (id, payload) => request(`/tickets/${id}`, { method: "PATCH", body: JSON.stringify(payload) }),
        addComment: (id, payload) => request(`/tickets/${id}/comments`, { method: "POST", body: JSON.stringify(payload) }),
        decideDuplicate: (id, payload) => request(`/duplicates/${id}/decision`, { method: "POST", body: JSON.stringify(payload) })
    };
})();
