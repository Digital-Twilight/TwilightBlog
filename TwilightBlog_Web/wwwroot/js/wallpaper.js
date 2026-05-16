window.wallpaperManager = {
    _current: 0,
    _images: [],
    _interval: null,
    _duration: 1.0,
    _dimming: 0.3,
    _blur: 0,
    _size: 'cover',
    _position: 'center',
    _activeLayer: 0,

    init: function (config, theme) {
        this._duration = config.transitionDuration ?? 1.0;
        this._dimming = config.dimming ?? 0.3;
        this._blur = config.blur ?? 0;
        this._size = config.size ?? 'cover';
        this._position = config.position ?? 'center';

        this.setTheme(theme, config);
    },

    setTheme: function (theme, config) {
        const set = theme === 'dark' ? config.dark : config.light;
        this._images = set?.images ?? [];
        this._current = 0;

        if (this._interval) {
            clearInterval(this._interval);
            this._interval = null;
        }

        if (this._images.length === 0) {
            this._clearWallpaper();
            return;
        }

        this._applyWallpaper(this._images[0]);

        if (this._images.length > 1) {
            this._interval = setInterval(() => {
                this._current = (this._current + 1) % this._images.length;
                this._applyWallpaper(this._images[this._current]);
            }, (config.intervalSeconds ?? 30) * 1000);
        }
    },

    _applyWallpaper: function (url) {
        let container = document.getElementById('tb-wallpaper-container');
        if (!container) {
            container = document.createElement('div');
            container.id = 'tb-wallpaper-container';
            container.style.cssText = `
            position: fixed;
            inset: 0;
            z-index: -1;
            pointer-events: none;
        `;
            document.body.prepend(container);

            // Два слоя
            for (let i = 0; i < 2; i++) {
                const layer = document.createElement('div');
                layer.style.cssText = `
                position: absolute;
                inset: 0;
                background-size: ${this._size};
                background-position: ${this._position};
                background-repeat: no-repeat;
                transition: opacity ${this._duration}s ease;
                opacity: 0;
            `;
                if (this._blur > 0)
                    layer.style.filter = `blur(${this._blur}px)`;
                container.appendChild(layer);
            }
        }

        const layers = container.children;
        const next = this._activeLayer === 0 ? 1 : 0;

        // Загружаем новое фото в скрытый слой
        layers[next].style.backgroundImage = `url('${url}')`;
        layers[next].style.opacity = '1';
        layers[this._activeLayer].style.opacity = '0';

        this._activeLayer = next;

        // Overlay
        let overlay = document.getElementById('tb-wallpaper-overlay');
        if (!overlay) {
            overlay = document.createElement('div');
            overlay.id = 'tb-wallpaper-overlay';
            overlay.style.cssText = `
            position: fixed;
            inset: 0;
            z-index: 0;
            background: #000;
            pointer-events: none;
            opacity: ${this._dimming};
        `;
            document.body.prepend(overlay);
        }
    },

    _clearWallpaper: function () {
        const container = document.getElementById('tb-wallpaper-container');
        const overlay = document.getElementById('tb-wallpaper-overlay');
        if (container) container.remove();
        if (overlay) overlay.remove();
        this._activeLayer = 0;
    },

    destroy: function () {
        if (this._interval) clearInterval(this._interval);
        this._clearWallpaper();
    }
};