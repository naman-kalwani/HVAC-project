/**
 * Page: Standardized Error Screens (PB-001 / US001)
 * 403 (Access Denied), 404 (Not Found), 500 (System Error)
 */
window.Pages = window.Pages || {};

window.Pages["403"] = function() {
    return `
        <article class="card" style="max-width: 600px; margin: 40px auto; text-align: center; padding: 40px 30px;">
            <div style="font-size: 54px; margin-bottom: 12px; color: #ef4444;">403</div>
            <h2 style="font-size: 22px; margin-bottom: 8px;">Access Restricted (Forbidden)</h2>
            <p style="font-size: 14px; color: #64748b; line-height: 1.6; margin-bottom: 24px;">
                Your assigned role or vendor workspace does not permit access to this section. Please switch roles or contact your platform administrator.
            </p>
            <div style="display: flex; justify-content: center; gap: 10px;">
                <button class="btn primary" onclick="App.navigate('dashboard')">Return to Safe Dashboard</button>
                <button class="btn" onclick="Auth.logout()">Sign Out</button>
            </div>
        </article>
    `;
};

window.Pages["404"] = function() {
    return `
        <article class="card" style="max-width: 600px; margin: 40px auto; text-align: center; padding: 40px 30px;">
            <div style="font-size: 54px; margin-bottom: 12px; color: #0f172a;">404</div>
            <h2 style="font-size: 22px; margin-bottom: 8px;">Screen or Record Not Found</h2>
            <p style="font-size: 14px; color: #64748b; line-height: 1.6; margin-bottom: 24px;">
                The screen, facility location, or equipment asset you are searching for does not exist or has been relocated.
            </p>
            <button class="btn primary" onclick="App.navigate('dashboard')">Back to Dashboard</button>
        </article>
    `;
};

window.Pages["500"] = function() {
    return `
        <article class="card" style="max-width: 600px; margin: 40px auto; text-align: center; padding: 40px 30px;">
            <div style="font-size: 54px; margin-bottom: 12px; color: #f59e0b;">500</div>
            <h2 style="font-size: 22px; margin-bottom: 8px;">Internal Service Exception</h2>
            <p style="font-size: 14px; color: #64748b; line-height: 1.6; margin-bottom: 24px;">
                An unexpected condition was encountered. Error details have been logged to the audit stream. Server internals are hidden for security.
            </p>
            <button class="btn primary" onclick="App.navigate('dashboard')">Return to Application</button>
        </article>
    `;
};

