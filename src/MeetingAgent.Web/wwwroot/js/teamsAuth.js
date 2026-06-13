(() => {
    const signInUrl = () => {
        const redirectUri = encodeURIComponent(window.location.pathname + window.location.search);
        return `/MicrosoftIdentity/Account/SignIn?redirectUri=${redirectUri}`;
    };

    const initializeTeams = async () => {
        if (!window.microsoftTeams?.app) {
            return false;
        }

        try {
            await window.microsoftTeams.app.initialize();
            return true;
        } catch {
            return false;
        }
    };

    const getTeamsToken = async () => {
        if (!window.microsoftTeams?.authentication?.getAuthToken) {
            return {
                token: null,
                error: "Teams authentication API is not available in this host."
            };
        }

        try {
            return {
                token: await window.microsoftTeams.authentication.getAuthToken(),
                error: null
            };
        } catch (error) {
            console.error("Teams SSO token acquisition failed.", error);
            return {
                token: null,
                error: formatTeamsError(error)
            };
        }
    };

    const formatTeamsError = (error) => {
        if (!error) {
            return "Unknown Teams SSO error.";
        }

        if (typeof error === "string") {
            return error;
        }

        const code = error.errorCode ?? error.code;
        const message = error.message ?? error.errorMessage;
        return [code ? `code=${code}` : null, message].filter(Boolean).join(": ") || JSON.stringify(error);
    };

    const fetchWithAuth = async (url, options = {}) => {
        const isTeamsHost = await initializeTeams();
        const tokenResult = isTeamsHost ? await getTeamsToken() : { token: null, error: null };
        const token = tokenResult.token;
        const headers = new Headers(options.headers ?? {});

        if (token) {
            headers.set("Authorization", `Bearer ${token}`);
        }

        const response = await fetch(url, {
            ...options,
            credentials: "include",
            headers
        });

        return {
            isTeamsHost,
            response,
            tokenAvailable: Boolean(token),
            tokenError: tokenResult.error
        };
    };

    const initializeAuthProbe = async (statusElementId) => {
        const statusElement = document.getElementById(statusElementId);
        if (!statusElement) {
            return;
        }

        const { isTeamsHost, response, tokenAvailable, tokenError } = await fetchWithAuth("/api/auth/me");
        if (response.ok) {
            statusElement.hidden = true;
            return;
        }

        statusElement.hidden = false;
        if (response.headers.get("X-MeetingAgent-Auth") === "NotConfigured") {
            statusElement.textContent = "Authentication is not configured for this local host.";
            return;
        }

        if (isTeamsHost || window.self !== window.top) {
            statusElement.textContent = tokenAvailable
                ? "Teams sign-in did not complete. Check the Entra app registration and Teams SSO manifest settings."
                : `Teams SSO token was not available. ${tokenError ?? "Check the Teams app registration and reload the tab."}`;
            return;
        }

        statusElement.innerHTML = `Sign in to continue setup. <a href="${signInUrl()}">Sign in</a>`;
    };

    window.meetingAgentAuth = {
        fetchWithAuth,
        getTeamsToken,
        initializeAuthProbe,
        initializeTeams,
        signInUrl
    };
})();
