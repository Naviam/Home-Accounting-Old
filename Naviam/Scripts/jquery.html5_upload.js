(function ($) {
    jQuery.fn.html5_upload = function (options) {

        var available_events = ['onStart', 'onStartOne', 'onProgress', 'onFinishOne', 'onFinish', 'onError'];
        var options = jQuery.extend({
            onStart: function (event, total) {
                return true;
            },
            onStartOne: function (event, name, number, total) {
                return true;
            },
            onProgress: function (event, progress, name, number, total) {
            },
            onFinishOne: function (event, response, name, number, total) {
            },
            onFinish: function (event, total) {
            },
            onError: function (event, name, error) {
            },
            autostart: true,
            autoclear: true,
            stopOnFirstError: false,

            STATUSES: {
                'STARTED': '������',
                'PROGRESS': '��������',
                'LOADED': '���������',
                'FINISHED': '���������'
            },

            setName: function (text) { },
            setStatus: function (text) { },
            setProgress: function (value) { },

            genName: function (file, number, total) {
                return file + "(" + (number + 1) + " �� " + total + ")";
            },
            genStatus: function (progress, finished) {
                if (finished) {
                    return options.STATUSES['FINISHED'];
                }
                if (progress == 0) {
                    return options.STATUSES['STARTED'];
                }
                else if (progress == 1) {
                    return options.STATUSES['LOADED'];
                }
                else {
                    return options.STATUSES['PROGRESS'];
                }
            },
            genProgress: function (loaded, total) {
                return loaded / total;
            }
        }, options);
        _isXHRUpload = function () {
            var undef = 'undefined';
            return typeof File !== undef && typeof FormData !== undef;
        };
        function upload() {
            var $this = $(this);
            var frm = $this.closest("form");
            if (frm.length > 0)
                options.url = frm[0].action;
            if (!_isXHRUpload()) {
                //post
                if (frm.length > 0)
                    frm.submit();
                return;
            }
            var files = this.files;
            var total = files.length;
            if (!$this.triggerHandler('html5_upload.onStart', [total])) {
                return false;
            }
            this.disabled = true;
            var uploaded = 0;
            var xhr = this.html5_upload['xhr'];
            this.html5_upload['continue_after_abort'] = true;
            function upload_file(number) {
                if (number == total) {
                    $this.trigger('html5_upload.onFinish', [total]);
                    options.setStatus(options.genStatus(1, true));
                    $this.attr("disabled", false);
                    if (options.autoclear) {
                        $this.val("");
                    }
                    return;
                }
                var file = files[number];
                if (!$this.triggerHandler('html5_upload.onStartOne', [file.fileName, number, total])) {
                    return upload_file(number + 1);
                }
                options.setStatus(options.genStatus(0));
                options.setName(options.genName(file.fileName, number, total));
                options.setProgress(options.genProgress(0, file.fileSize));
                xhr.upload['onprogress'] = function (rpe) {
                    $this.trigger('html5_upload.onProgress', [rpe.loaded / rpe.total, file.fileName, number, total]);
                    options.setStatus(options.genStatus(rpe.loaded / rpe.total));
                    options.setProgress(options.genProgress(rpe.loaded, rpe.total));
                };
                xhr.onload = function (load) {
                    $this.trigger('html5_upload.onFinishOne', [xhr.responseText, file.fileName, number, total]);
                    options.setStatus(options.genStatus(1, true));
                    options.setProgress(options.genProgress(file.fileSize, file.fileSize));
                    upload_file(number + 1);
                };
                xhr.onabort = function () {
                    if ($this[0].html5_upload['continue_after_abort']) {
                        upload_file(number + 1);
                    }
                    else {
                        $this.attr("disabled", false);
                        if (options.autoclear) {
                            $this.val("");
                        }
                    }
                };
                xhr.onerror = function (e) {
                    $this.trigger('html5_upload.onError', [file.fileName, e]);
                    if (!options.stopOnFirstError) {
                        upload_file(number + 1);
                    }
                };
                xhr.onreadystatechange = function (evt) {
                    if (xhr.readyState == 4) {
                        if (xhr.status == 500)
                            parseSiteError(xhr);
                    }
                };
                xhr.open("post", typeof (options.url) == "function" ? options.url() : options.url, true);
                //				xhr.setRequestHeader("Cache-Control", "no-cache");
                xhr.setRequestHeader("X-Requested-With", "XMLHttpRequest");
                //				xhr.setRequestHeader("X-File-Name", file.fileName);
                //				xhr.setRequestHeader("X-File-Size", file.fileSize);
                //				xhr.setRequestHeader("Content-Type", "multipart/form-data");
                var fd = new FormData();
                fd.append($this.attr("name"), file);
                xhr.send(fd);
            }
            upload_file(0);
            return true;
        }

        return this.each(function () {
            this.html5_upload = {
                xhr: new XMLHttpRequest(),
                continue_after_abort: true
            };
            if (options.autostart) {
                $(this).bind('change', upload);
            }
            for (var event in available_events) {
                if (options[available_events[event]]) {
                    $(this).bind("html5_upload." + available_events[event], options[available_events[event]]);
                }
            }
            $(this)
				.bind('html5_upload.start', upload)
				.bind('html5_upload.cancelOne', function () {
				    this.html5_upload['xhr'].abort();
				})
				.bind('html5_upload.cancelAll', function () {
				    this.html5_upload['continue_after_abort'] = false;
				    this.html5_upload['xhr'].abort();
				})
				.bind('html5_upload.destroy', function () {
				    this.html5_upload['continue_after_abort'] = false;
				    this.xhr.abort();
				    delete this.html5_upload;
				    $(this).unbind('html5_upload.*').unbind('change', upload);
				});
        });
    };
})(jQuery);
