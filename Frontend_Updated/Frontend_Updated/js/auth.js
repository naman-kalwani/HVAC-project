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

    /**
     * Authenticate with email and password
     */
    login(email, password, remember = false) {
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
    signup(name, email, password) {
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

        return newUser;
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
        localStorage.removeItem(AUTH_STORAGE_KEY);
        sessionStorage.removeItem(AUTH_STORAGE_KEY);
        window.location.replace("index.html");
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
