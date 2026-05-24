window.scrollbarManager = {
    _thumb: null,
    _track: null,
    _isDragging: false,
    _startY: 0,
    _startScroll: 0,
    _hideTimeout: null,

    init: function () {
        this._track = document.createElement('div');
        this._track.id = 'tb-scrollbar-track';

        this._thumb = document.createElement('div');
        this._thumb.id = 'tb-scrollbar-thumb';

        this._track.appendChild(this._thumb);
        document.body.appendChild(this._track);

        this._updateBounds();
        this._update();

        window.addEventListener('scroll', () => {
            this._updateBounds();
            this._update();
            this._show();
            this._scheduleHide();
        }, { passive: true });

        window.addEventListener('resize', () => {
            this._updateBounds();
            this._update();
        }, { passive: true });

        this._thumb.addEventListener('mousedown', (e) => this._onDragStart(e));
        document.addEventListener('mousemove', (e) => this._onDragMove(e));
        document.addEventListener('mouseup', () => this._onDragEnd());

        this._track.addEventListener('click', (e) => {
            if (e.target === this._track) {
                const rect = this._track.getBoundingClientRect();
                const ratio = (e.clientY - rect.top) / rect.height;
                window.scrollTo({
                    top: ratio * (document.documentElement.scrollHeight - window.innerHeight),
                    behavior: 'smooth'
                });
            }
        });

        window.addEventListener('wheel', (e) => {
            e.preventDefault();
            window.scrollBy({
                top: e.deltaY * 1.5,
                behavior: 'smooth'
            });
        }, { passive: false });

        this._scheduleHide();
    },

    _updateBounds: function () {
        const nav = document.querySelector('.tb-nav');
        const footer = document.querySelector('.tb-footer');

        const navBottom = nav ? nav.getBoundingClientRect().bottom : 0;

        const footerRect = footer ? footer.getBoundingClientRect() : null;
        const footerVisible = footerRect && footerRect.top < window.innerHeight;
        const trackBottom = footerVisible ? footerRect.top : window.innerHeight;

        this._track.style.top = (navBottom + 4) + 'px';
        this._track.style.height = (trackBottom - navBottom - 8) + 'px';
    },

    _update: function () {
        const scrollHeight = document.documentElement.scrollHeight;
        const clientHeight = window.innerHeight;
        const scrollTop = window.scrollY;
        const trackHeight = parseFloat(this._track.style.height) || clientHeight;

        if (scrollHeight <= clientHeight) {
            this._track.style.opacity = '0';
            return;
        }

        this._track.style.opacity = '';

        const thumbHeight = Math.max(40, (clientHeight / scrollHeight) * trackHeight);
        const maxThumbTop = trackHeight - thumbHeight;
        const thumbTop = (scrollTop / (scrollHeight - clientHeight)) * maxThumbTop;

        this._thumb.style.height = thumbHeight + 'px';
        this._thumb.style.transform = 'translateY(' + thumbTop + 'px)';
    },

    _show: function () {
        this._track.classList.add('visible');
    },

    _scheduleHide: function () {
        if (this._hideTimeout) clearTimeout(this._hideTimeout);
        this._hideTimeout = setTimeout(() => {
            if (!this._isDragging)
                this._track.classList.remove('visible');
        }, 1200);
    },

    _onDragStart: function (e) {
        this._isDragging = true;
        this._startY = e.clientY;
        this._startScroll = window.scrollY;
        this._track.classList.add('dragging');
        e.preventDefault();
    },

    _onDragMove: function (e) {
        if (!this._isDragging) return;

        const scrollHeight = document.documentElement.scrollHeight;
        const clientHeight = window.innerHeight;
        const thumbHeight = parseFloat(this._thumb.style.height);

        const delta = e.clientY - this._startY;
        const scrollDelta = delta / (clientHeight - thumbHeight) * (scrollHeight - clientHeight);

        window.scrollTo(0, this._startScroll + scrollDelta);
    },

    _onDragEnd: function () {
        if (!this._isDragging) return;
        this._isDragging = false;
        this._track.classList.remove('dragging');
        this._scheduleHide();
    }
};