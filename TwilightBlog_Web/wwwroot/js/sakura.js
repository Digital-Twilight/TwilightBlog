window.sakuraManager = {
    _canvas: null,
    _ctx: null,
    _petals: [],
    _animationId: null,
    _config: null,

    init: function (config) {
        this._config = config;

        if (this._canvas) {
            this.destroy();
        }

        this._canvas = document.createElement('canvas');
        this._canvas.id = 'sakura-canvas';
        this._canvas.style.cssText = `
            position: fixed;
            inset: 0;
            z-index: 1;
            pointer-events: none;
        `;
        document.body.appendChild(this._canvas);

        this._resize();
        window.addEventListener('resize', () => this._resize());

        for (let i = 0; i < config.count; i++) {
            this._petals.push(this._createPetal(true));
        }

        this._animate();
    },

    _resize: function () {
        if (!this._canvas) return;
        this._canvas.width = window.innerWidth;
        this._canvas.height = window.innerHeight;
        this._ctx = this._canvas.getContext('2d');
    },

    _createPetal: function (randomY = false) {
        const config = this._config;
        const size = config.minSize + Math.random() * (config.maxSize - config.minSize);
        return {
            x: Math.random() * window.innerWidth,
            y: randomY ? Math.random() * window.innerHeight : -size,
            size: size,
            speedY: config.minSpeed + Math.random() * (config.maxSpeed - config.minSpeed),
            speedX: (Math.random() - 0.5) * 1.5,
            rotation: Math.random() * Math.PI * 2,
            rotationSpeed: (Math.random() - 0.5) * 0.05,
            opacity: 0.4 + Math.random() * 0.6,
            swing: Math.random() * Math.PI * 2,
            swingSpeed: 0.01 + Math.random() * 0.02,
            swingRadius: 20 + Math.random() * 40,
        };
    },

    _drawPetal: function (petal) {
        const ctx = this._ctx;
        const config = this._config;

        ctx.save();
        ctx.translate(petal.x, petal.y);
        ctx.rotate(petal.rotation);
        ctx.globalAlpha = petal.opacity * config.opacity;

        ctx.beginPath();
        ctx.moveTo(0, 0);
        ctx.bezierCurveTo(
            petal.size / 2, -petal.size / 2,
            petal.size, -petal.size / 4,
            petal.size, 0
        );
        ctx.bezierCurveTo(
            petal.size, petal.size / 4,
            petal.size / 2, petal.size / 2,
            0, 0
        );

        ctx.fillStyle = config.color;
        ctx.fill();

        ctx.restore();
    },

    _animate: function () {
        if (!this._ctx || !this._canvas) return;

        this._ctx.clearRect(0, 0, this._canvas.width, this._canvas.height);

        for (let i = 0; i < this._petals.length; i++) {
            const p = this._petals[i];

            p.swing += p.swingSpeed;
            p.x += p.speedX + Math.sin(p.swing) * 0.8;
            p.y += p.speedY;
            p.rotation += p.rotationSpeed;

            if (p.y > window.innerHeight + p.size ||
                p.x < -p.size * 2 ||
                p.x > window.innerWidth + p.size * 2) {
                this._petals[i] = this._createPetal(false);
            }

            this._drawPetal(p);
        }

        this._animationId = requestAnimationFrame(() => this._animate());
    },

    destroy: function () {
        if (this._animationId) {
            cancelAnimationFrame(this._animationId);
            this._animationId = null;
        }
        if (this._canvas) {
            this._canvas.remove();
            this._canvas = null;
        }
        this._petals = [];
    }
};