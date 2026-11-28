# HVAC Connect Visual Redesign Plan

## Goal

Create a cohesive, polished interface across the public landing page, authentication screens, and role-based operations app. Preserve the current routes, content, role visibility, and working interactions. This document is a proposal only; no visual implementation should begin until approved.

## Design read

HVAC Connect is a B2B operations product for facilities teams, service coordinators, technicians, vendors, and administrators. The visual direction is professional, calm, and operational: clear hierarchy, confident navy, restrained teal, and purposeful motion rather than decorative effects.

## Current state

- The project is framework-free HTML, CSS, and vanilla JavaScript.
- The authenticated app loads `main.css` and `components.css`; the public landing page has its own styling in `style.css`; authentication uses `auth.css`.
- `main.css` and `components.css` contain overlapping component rules, while `align.css` and `responsive.css` provide additional styles that are not loaded by `app.html`.
- The app already has reusable UI helpers for tables, KPI cards, badges, pagination, and notifications, but page modules use inconsistent inline styles and locally defined structures.
- The landing page uses navy and teal with Manrope and DM Sans. The operations app uses a separate palette and a system font stack.
- The same landing content appears in `index.html` and `landing-page.html`; their stylesheet paths differ.

## Design settings

- **Design variance: 4/10.** Retain predictable navigation and page structure, adding modest visual distinction through spacing, surface depth, and selected layout variation.
- **Motion intensity: 4/10.** Use short transitions for navigation, card state changes, and button feedback. Avoid continuous or attention-seeking animation.
- **Visual density: 5/10.** Keep operational information easy to scan without turning the app into a sparse marketing site or an overloaded cockpit.

## Proposed visual system

### Foundation

- Keep the existing HTML/CSS/JavaScript architecture. Do not introduce React, Tailwind, or a component-library migration for this styling pass.
- Establish one shared set of CSS custom properties for colors, typography, spacing, borders, radii, shadows, and motion timing.
- Use the existing brand direction as the source: navy and teal with cool, neutral surfaces. Unify the app and public pages rather than introducing a second palette.
- Standardize on the existing Manrope heading and DM Sans body pairing, with system-font fallbacks.
- Use a consistent shape rule: 12px cards, 8px controls, and pill shapes only for compact status labels.
- Keep status colors semantic and consistent: green for resolved/success, amber for at-risk/pending, red for breached/error, and blue for informational states.

### Preliminary token direction

These are starting values for implementation and will be checked for contrast before finalizing:

| Token | Direction |
|---|---|
| App background | Cool light gray, approximately `#F3F6F8` |
| Main surface | White, with a lightly tinted secondary surface |
| Primary text | Deep navy, approximately `#102A3A` |
| Muted text | Blue-gray with readable contrast |
| Brand primary | Existing deep navy, approximately `#123B5D` |
| Brand accent | Existing teal, approximately `#087E8B` |
| Borders | Soft blue-gray, approximately `#D8E2E9` |
| Shadows | Subtle navy-tinted shadows used only to clarify elevation |

### Shared components

Apply the same visual rules to shared and repeated UI:

- App header, brand treatment, user menu, and role-based sidebar
- Page headings, descriptions, action groups, and breadcrumbs
- Cards, KPI tiles, dashboards, and status summaries
- Search/filter toolbars, tables, card grids, and empty states
- Buttons, inputs, selects, text areas, badges, and progress indicators
- Pagination, toasts, confirmation messages, and modal dialogs
- Loading and error states where an existing interaction needs them

Prefer consistent component classes and the existing `UI` helpers over page-specific inline styling. Cards should group related information; avoid wrapping every small element in a separate card.

## Page coverage

1. **Shared app shell:** unify desktop header/sidebar spacing, selected navigation, page width, content rhythm, and mobile navigation behavior.
2. **Dashboards:** give KPI summaries, dispatch queues, ticket snapshots, and operational alerts a common hierarchy while retaining role-specific information.
3. **Directories and registries:** standardize vendor, customer, location, asset, technician, and ticket list/card layouts, including their filters, pagination, and actions.
4. **Workflow pages:** apply the same typography, surfaces, forms, progress states, and feedback patterns to imports, duplicate review, onboarding, audit, and detail pages.
5. **Authentication:** align login, registration, and password recovery with the product palette and form system. Preserve the removal of demo-login controls.
6. **Public landing pages:** align `index.html` and `landing-page.html` with the shared brand tokens while preserving their existing information architecture and content.
7. **Error and modal states:** make 403/404/500 views, dialogs, and transient notifications visually consistent with the app.

## Layout and responsive behavior

- Desktop: retain the fixed navigation structure, with a compact top bar, clear sidebar selection, and a centered content width that prevents tables and cards from stretching excessively.
- Tablet: reduce page padding and allow KPI and card grids to collapse cleanly.
- Mobile: use a compact header and accessible navigation drawer; stack card grids and forms; keep tables usable with horizontal scrolling where conversion to cards is not appropriate.
- Ensure long labels, empty states, buttons, filters, and modals remain usable at narrow widths.
- Preserve current page keys, navigation labels, form field order, and role-specific controls.

## Motion and accessibility

- Use short opacity/transform transitions for route content, card hover/focus, menus, and dialogs where motion improves state feedback.
- Respect `prefers-reduced-motion` by removing or minimizing non-essential transitions.
- Provide visible keyboard focus, semantic interactive elements, clear disabled states, and sufficient text/control contrast.
- Do not use motion to communicate information that is otherwise unavailable.
- Avoid perpetual animation, parallax, scroll hijacking, and large decorative effects.

## Planned implementation sequence

1. Consolidate and document the shared design tokens and base styles.
2. Refine the authenticated app shell and shared components.
3. Apply the shared components across the dashboard and data-management pages.
4. Align authentication and public landing styles with the same brand system.
5. Tune responsive layouts, empty states, focus behavior, and reduced-motion handling.
6. Validate all roles and routes at desktop, tablet, and mobile sizes; verify key interactions and check color contrast.

## Likely files to update after approval

- `css/main.css`, `css/components.css`, `css/responsive.css`, `css/style.css`, `css/auth.css`, and possibly `css/align.css`
- `app.html`, `auth.html`, `index.html`, and `landing-page.html` for shared style references or semantic class hooks
- `js/ui.js` and selected files under `pages/` only where shared component markup needs consistent hooks

Functional logic, mock data, authentication behavior, route names, and role permissions are out of scope unless a styling change exposes a directly related defect.

## Acceptance criteria

- The public pages, authentication screens, and authenticated pages look like one product.
- Repeated components have consistent spacing, typography, color, radius, elevation, and interaction states.
- Existing routes and role-based content continue to work without functional regression.
- Layouts remain usable on desktop, tablet, and mobile.
- Motion is restrained, purposeful, and reduced when the user requests reduced motion.
- Keyboard focus and text/control contrast remain clear.

## Approval gate

Wait for explicit approval before editing styles, markup, or page modules for the visual redesign. The next step after approval is to implement this plan in small, verifiable passes.
