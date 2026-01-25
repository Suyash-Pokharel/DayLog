window.daylogQuill = window.daylogQuill || {};

window.daylogQuill.init = (elementId, initialHtml, placeholder, readOnly, dotNetRef) => {
    try {
        const el = document.getElementById(elementId);
        if (!el) return;

        // If already initialized, update content and return
        if (window.daylogQuill[elementId]) {
            const existing = window.daylogQuill[elementId];
            existing.root.innerHTML = initialHtml || "";
            existing.enable(!readOnly);
            return;
        }

        const toolbarOptions = [
            ['bold', 'italic', 'underline', 'strike'],
            [{ 'header': 1 }, { 'header': 2 }],
            [{ 'list': 'ordered' }, { 'list': 'bullet' }],
            ['blockquote', 'code-block'],
            ['link', 'image'],
            [{ 'color': [] }, { 'background': [] }],
            [{ 'align': [] }],
            ['clean']
        ];

        const q = new Quill('#' + elementId, {
            modules: { toolbar: toolbarOptions },
            theme: 'snow',
            placeholder: placeholder || ''
        });

        q.root.innerHTML = initialHtml || "";
        if (readOnly) q.disable();

        // store editor instance
        window.daylogQuill[elementId] = q;

        // register change callback if dotNetRef provided
        if (dotNetRef) {
            const handler = () => {
                try {
                    const html = q.root.innerHTML;
                    dotNetRef.invokeMethodAsync('NotifyContentChanged', html).catch(e => console.error(e));
                } catch (err) {
                    console.error('Quill change handler error:', err);
                }
            };
            q.on('text-change', handler);

            // keep handler reference for destroy cleanup
            window.daylogQuill[elementId + '_handler'] = handler;
        }
    } catch (e) {
        console.error('daylogQuill.init error:', e);
    }
};

window.daylogQuill.getHtml = (elementId) => {
    try {
        const q = window.daylogQuill[elementId];
        if (!q) return "";
        return q.root.innerHTML;
    } catch (e) {
        console.error("daylogQuill.getHtml error:", e);
        return "";
    }
};

window.daylogQuill.setHtml = (elementId, html) => {
    try {
        const q = window.daylogQuill[elementId];
        if (!q) return;
        q.root.innerHTML = html || "";
    } catch (e) {
        console.error("daylogQuill.setHtml error:", e);
    }
};

window.daylogQuill.destroy = (elementId) => {
    try {
        const q = window.daylogQuill[elementId];
        if (!q) return;

        // remove event handler if stored
        const handler = window.daylogQuill[elementId + '_handler'];
        if (handler) {
            try {
                q.off('text-change', handler);
            } catch (_) { }
            delete window.daylogQuill[elementId + '_handler'];
        }

        // attempt to clear and delete
        q.root.innerHTML = "";
        try {
            delete window.daylogQuill[elementId];
        } catch (e) { window.daylogQuill[elementId] = null; }
    } catch (e) {
        console.error('daylogQuill.destroy error:', e);
    }
};
