window.musicPlayer = {
    _audio: null,
    _tracks: [],
    _currentIndex: 0,
    _isPlaying: false,
    _volume: 0.5,
    _shuffle: false,
    _dotnetRef: null,

    init: async function (config, dotnetRef) {
        const savedVolume = localStorage.getItem('player-volume');
        this._volume = savedVolume !== null ? parseFloat(savedVolume) : (config.volume ?? 0.5);
        this._shuffle = config.shuffle ?? false;
        this._dotnetRef = dotnetRef;

        this._audio = new Audio();
        this._audio.volume = this._volume;

        document.addEventListener('input', (e) => {
            if (e.target.classList.contains('player-progress')) {
                this._audio.currentTime = parseFloat(e.target.value);
            }
            if (e.target.classList.contains('player-volume')) {
                this.setVolume(parseFloat(e.target.value));
            }
        });

        this._audio.addEventListener('timeupdate', () => {
            const progress = document.querySelector('.player-progress');
            if (progress) {
                progress.max = this._audio.duration || 0;
                progress.value = this._audio.currentTime;
            }
            if (this._dotnetRef) {
                this._dotnetRef.invokeMethodAsync('OnTimeUpdate',
                    this._audio.currentTime,
                    this._audio.duration || 0);
            }
        });

        this._audio.addEventListener('ended', () => this.next());

        this._audio.addEventListener('play', () => {
            this._isPlaying = true;
            if (this._dotnetRef)
                this._dotnetRef.invokeMethodAsync('OnPlayStateChanged', true);
        });

        this._audio.addEventListener('pause', () => {
            this._isPlaying = false;
            if (this._dotnetRef)
                this._dotnetRef.invokeMethodAsync('OnPlayStateChanged', false);
        });

        document.addEventListener('wheel', (e) => {
            const tracklist = document.querySelector('.player-tracklist');
            if (tracklist && tracklist.contains(e.target)) {
                e.stopPropagation();
            }
        }, { passive: true });

        await this._loadTrackList();

        if (config.autoplay && this._tracks.length > 0)
            await this._playIndex(0);
    },

    _loadTrackList: async function () {
        const response = await fetch('/api/music/tracks');
        if (!response.ok) return;

        const tracks = await response.json();

        this._tracks = tracks.map(t => ({
            url: t.url,
            title: t.title,
            artist: t.artist,
            album: t.album,
            duration: t.duration,
            cover: t.hasCover ? `/api/music/cover/${encodeURIComponent(t.fileName)}` : null,
            tagsLoaded: true
        }));

        if (this._dotnetRef)
            this._dotnetRef.invokeMethodAsync('OnTracksLoaded',
                JSON.stringify(this._tracks));
    },

    _playIndex: async function (index) {
        this._currentIndex = index;
        this._audio.src = this._tracks[index].url;
        this._audio.play();
        if (this._dotnetRef)
            this._dotnetRef.invokeMethodAsync('OnTrackChanged', index);
    },

    _readTags: function (url) {
        return new Promise(async (resolve) => {
            try {
                const response = await fetch(url);
                const arrayBuffer = await response.arrayBuffer();

                jsmediatags.read(new Blob([arrayBuffer]), {
                    onSuccess: (tag) => {
                        const tags = tag.tags;
                        let coverUrl = null;

                        if (tags.picture) {
                            const pic = tags.picture;
                            const blob = new Blob(
                                [new Uint8Array(pic.data)],
                                { type: pic.format }
                            );
                            coverUrl = URL.createObjectURL(blob);
                        }

                        resolve({
                            url: url,
                            title: tags.title || url.split('/').pop().replace('.mp3', ''),
                            artist: tags.artist || 'Unknown',
                            album: tags.album || '',
                            cover: coverUrl
                        });
                    },
                    onError: () => {
                        resolve({
                            url: url,
                            title: decodeURIComponent(url.split('/').pop().replace('.mp3', '')),
                            artist: 'Unknown',
                            album: '',
                            cover: null
                        });
                    }
                });
            } catch {
                resolve({
                    url: url,
                    title: decodeURIComponent(url.split('/').pop().replace('.mp3', '')),
                    artist: 'Unknown',
                    album: '',
                    cover: null
                });
            }
        });
    },

    play: function () {
        if (this._tracks.length === 0) return;
        if (!this._audio.src || this._audio.src === window.location.href) {
            this._playIndex(this._currentIndex);
        } else {
            this._audio.play();
        }
    },

    pause: function () {
        this._audio.pause();
    },

    toggle: function () {
        if (this._isPlaying) this.pause();
        else this.play();
    },

    next: function () {
        if (this._tracks.length === 0) return;

        if (!this._shuffle) {
            const index = (this._currentIndex + 1) % this._tracks.length;
            this._playIndex(index);
            return;
        }

        // если очередь пуста — пересобираем
        if (!this._shuffleQueue || this._shuffleQueue.length === 0) {
            this._buildShuffleQueue();
        }

        let index = this._shuffleQueue.pop();

        // защита от undefined
        if (index === undefined) {
            index = Math.floor(Math.random() * this._tracks.length);
        }

        // защита от повтора текущего трека (только если есть альтернатива)
        if (this._tracks.length > 1 && index === this._currentIndex) {
            index = this._shuffleQueue.pop();

            if (index === undefined || index === this._currentIndex) {
                index = (this._currentIndex + 1) % this._tracks.length;
            }
        }

        this._playIndex(index);
    },

    _buildShuffleQueue: function () {
        const arr = [...Array(this._tracks.length).keys()];

        for (let i = arr.length - 1; i > 0; i--) {
            const j = Math.floor(Math.random() * (i + 1));
            [arr[i], arr[j]] = [arr[j], arr[i]];
        }

        this._shuffleQueue = arr;
    },

    prev: function () {
        if (this._tracks.length === 0) return;
        if (this._audio.currentTime > 3) {
            this._audio.currentTime = 0;
            return;
        }
        const index = (this._currentIndex - 1 + this._tracks.length) % this._tracks.length;
        this._playIndex(index);
    },

    seek: function (time) {
        this._audio.currentTime = time;
    },

    setVolume: function (volume) {
        this._volume = volume;
        this._audio.volume = volume;
        localStorage.setItem('player-volume', volume);
    },

    setShuffle: function (shuffle) {
        this._shuffle = shuffle;

        if (shuffle) {
            this._buildShuffleQueue();
        } else {
            this._shuffleQueue = [];
        }
    },

    selectTrack: function (index) {
        this._playIndex(index);
    },

    getState: function () {
        return {
            isPlaying: this._isPlaying,
            currentIndex: this._currentIndex,
            volume: this._volume,
            shuffle: this._shuffle
        };
    },

    destroy: function () {
        if (this._audio) {
            this._audio.pause();
            this._audio.src = '';
        }
        this._dotnetRef = null;
    },

    _updateSliders: function () {
        const progress = document.querySelector('.player-progress');
        const volume = document.querySelector('.player-volume');

        if (progress && this._audio) {
            progress.max = this._audio.duration || 0;
            progress.value = this._audio.currentTime;
        }

        if (volume) {
            volume.value = this._volume;
        }
    },

    initSliders: function () {
        requestAnimationFrame(() => {
            requestAnimationFrame(() => {
                const volume = document.querySelector('.player-volume');
                const progress = document.querySelector('.player-progress');

                if (volume) {
                    volume.min = 0;
                    volume.max = 1;
                    volume.step = 0.01;
                    volume.value = this._volume;
                }

                if (progress) {
                    progress.min = 0;
                    progress.max = this._audio ? (this._audio.duration || 0) : 0;
                    progress.step = 0.1;
                    progress.value = this._audio ? this._audio.currentTime : 0;
                }
            });
        });
    }
};