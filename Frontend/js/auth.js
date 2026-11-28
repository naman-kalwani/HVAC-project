/**
 * HVAC Connect - Authentication & Session Manager
 * Handles login, session state, role switching and navigation guards.
 */

const DEMO_USERS = [
    {
        email: "admin@gmail.com",
        password: "Password123",
        name: "Shivansh Rastogi",
        role: "Super Admin",
        roleKey: "superadmin",
        avatar: "SR",
        workspace: "Parent Company",
        title: "Group Operations Director"
    },
    {
        email: "vendor@gmail.com",
        password: "Password123",
        name: "Jordan Lee",
        role: "Vendor Admin",
        roleKey: "vendoradmin",
        avatar: "JL",
        workspace: "Pacific Climate Services",
        vendorId: "PCS-001",
        title: "Branch General Manager"
    },
    {
        email: "Service@gmail.com",
        password: "Password123",
        name: "Taylor Chen",
        role: "Service Coordinator",
        roleKey: "coordinator",
        avatar: "TC",
        workspace: "Regional Dispatch Desk",
        title: "Senior Service Coordinator"
    },
    {
        email: "technician@gmail.com",
        password: "Password123",
        name: "Casey Reed",
        role: "Technician",
        roleKey: "technician",
        avatar: "CR",
        workspace: "My Work (Field App)",
        employeeId: "TECH-104",
        specialization: "HVAC / Chillers",
        title: "Lead Commercial HVAC Specialist"
    },
    {
        email: "user@gmail.com",
        password: "Password123",
        name: "Morgan Ellis",
        role: "End User",
        roleKey: "enduser",
        avatar: "ME",
        workspace: "Apex Retail Group",
        title: "Corporate Facilities Director"
    }
];

const AUTH_STORAGE_KEY = "hvac_connect_current_user";
const ROLE_KEYS = { SuperAdmin: "superadmin", VendorAdmin: "vendoradmin", ServiceCoordinator: "coordinator", Technician: "technician", EndUser: "enduser" };
const ROLE_LABELS = { SuperAdmin: "Super Admin", VendorAdmin: "Vendor Admin", ServiceCoordinator: "Service Coordinator", Technician: "Technician", EndUser: "End User" };

function toSessionUser(user) {
    return { ...user, roleKey: ROLE_KEYS[user.role] || "enduser", role: ROLE_LABELS[user.role] || user.role,
        avatar: (user.name || "U").split(" ").map(x => x[0]).join("").slice(0, 2).toUpperCase(),
        workspace: user.vendorName || user.customerName || "Registered Client Facility" };
}

const Auth = {
    /**
     * Get currently logged-in user from localStorage, or null
     */
    getCurrentUser() {
        try {
            const raw = sessionStorage.getItem(AUTH_STORAGE_KEY) ||
                localStorage.getItem(AUTH_STORAGE_KEY);
            return raw ? JSON.parse(raw) : null;
        } catch (e) {
            console.error("Failed to parse user session", e);
            return null;
        }
    },

    async refreshSession() {
        const current = this.getCurrentUser();
        if (!current || !window.HvacApi || !current.id) return current;
        try {
            const profile = await window.HvacApi.me(current.id);
            const updated = toSessionUser(profile);
            this.setSession(updated, localStorage.getItem(AUTH_STORAGE_KEY) !== null);
            return updated;
        } catch (error) {
            this.clearSession();
            return null;
        }
    },

    /**
     * Authenticate with email and password
     */
    async login(email, password, remember = false) {
        if (window.HvacApi) {
            try {
                const result = await window.HvacApi.login(email, password);
                const user = toSessionUser(result.user);
                this.setSession(user, remember);
                return { success: true, user };
            } catch (error) {
                return { success: false, error: error.message || "Unable to connect to the API." };
            }
        }
        const trimmedEmail = (email || "").trim().toLowerCase();
        const user = DEMO_USERS.find(u => u.email.toLowerCase() === trimmedEmail);

        if (!user) {
            return { success: false, error: "No account found with this email address." };
        }

        if (user.password !== password) {
            return { success: false, error: "Incorrect password. Please try again." };
        }

        this.setSession(user, remember);
        return { success: true, user };
    },

    /**
     * Register a new user
     */
    async signup(name, email, password) {
        if (window.HvacApi) {
            try {
                const result = await window.HvacApi.signup(name, email, password);
                const user = toSessionUser(result.user);
                this.setSession(user, false);
                return { success: true, user };
            } catch (error) {
                return { success: false, error: error.message || "Unable to create account." };
            }
        }
        const initials = name.split(" ").map(p => p[0]).join("").toUpperCase().slice(0, 2) || "U";

        const newUser = {
            name,
            email,
            password,
            role: "End User",
            roleKey: "enduser",
            avatar: initials,
            workspace: "Registered Client Facility",
            employeeId: null,
            title: "End User Account"
        };

        DEMO_USERS.push(newUser);
        this.setSession(newUser);App.showToast('Sign up successfull', 'success');
        window.location.replace("auth.html");

        return { success: true, user: newUser };
    },

    /**
     * Persist user session
     */
    setSession(user, remember = false) {
        localStorage.removeItem(AUTH_STORAGE_KEY);
        sessionStorage.removeItem(AUTH_STORAGE_KEY);
        const storage = remember ? localStorage : sessionStorage;
        storage.setItem(AUTH_STORAGE_KEY, JSON.stringify(user));
    },

    /**
     * Clear user session and redirect to login
     */
    logout() {
        this.clearSession();
        window.location.replace("index.html");
    },

    clearSession() {
        localStorage.removeItem(AUTH_STORAGE_KEY);
        sessionStorage.removeItem(AUTH_STORAGE_KEY);
    },

    /**
     * Navigation guard for protected pages
     */
    requireAuth() {
        const user = this.getCurrentUser();
        if (!user) {
            window.location.replace("index.html");
            return null;
        }
        return user;
    },

    /**
     * Get default landing page for user's role
     */
    getDefaultLandingPage(user) {
        if (!user) return "dashboard";
        switch (user.roleKey) {
            case "vendoradmin": return "vendor-dashboard";
            case "coordinator": return "coordinator-dashboard";
            case "technician": return "technician-dashboard";
            case "enduser": return "enduser-dashboard";
            case "superadmin":
            default:
                return "dashboard";
        }
    }
};

// Export to window for vanilla JS access across script tags
window.Auth = Auth;
