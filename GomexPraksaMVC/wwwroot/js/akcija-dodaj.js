document.addEventListener('DOMContentLoaded', function () {
    (function () {
        var input = document.querySelector('input[name="SifraArtikla"]');
        if (!input) return;

        var preview = document.getElementById('article-preview');
        var nameEl = document.getElementById('article-name');
        var priceEl = document.getElementById('article-price');
        var regularPrice = null;
        var ignoreCalc = false; 
        var akcijskaInput = document.getElementById('akcijska-cena');
        var discountInput = document.getElementById('discount-percent');
        var timeout = null;
        var suggestionsEl = document.getElementById('article-suggestions');
        var tipSelect = document.querySelector('select[name="TipAkcijeId"]');
        var clientErrorEl = document.getElementById('sifra-client-error');
        var articleExists = null; // true|false|null

        function clearPreview() {
            if (!preview) return;
            preview.style.display = 'none';
            nameEl.textContent = '';
            priceEl.textContent = '';
            regularPrice = null;
        }

        function showLoading() {
            if (!preview) return;
            preview.style.display = 'block';
            nameEl.textContent = 'Učitavanje...';
            priceEl.textContent = '';
        }

        function fetchArticle(sifra) {
            if (!sifra) { clearPreview(); articleExists = null; if (clientErrorEl) clientErrorEl.textContent = ''; return Promise.resolve(false); }
            sifra = sifra.trim().toUpperCase();
            showLoading();
            if (clientErrorEl) clientErrorEl.textContent = '';
            var url = '/Akcija/LookupArtikal?sifra=' + encodeURIComponent(sifra);

            return fetch(url, { credentials: 'same-origin' })
                .then(function (resp) {
                    if (resp.status === 404) {
                        articleExists = false;
                        if (clientErrorEl) clientErrorEl.textContent = 'Artikal sa unetom šifrom nije pronađen.';
                        nameEl.textContent = 'Artikal nije pronađen (404)';
                        priceEl.textContent = '';
                        return null;
                    }

                    if (!resp.ok) {
                        return resp.text().then(function (t) { throw new Error('Server error ' + resp.status + ' - ' + (t || '')); });
                    }

                    var ct = resp.headers.get('content-type') || '';
                    if (ct.indexOf('application/json') === -1) {
                        return resp.text().then(function (t) {
                            articleExists = false;
                            if (clientErrorEl) clientErrorEl.textContent = 'Neočekivan odgovor servera.';
                            nameEl.textContent = 'Server returned unexpected response';
                            priceEl.textContent = t ? t.substring(0, 200) : '';
                            return null;
                        });
                    }

                    return resp.json();
                })
                .then(function (data) {
                    if (!data) return false;
                    articleExists = true;
                    if (clientErrorEl) clientErrorEl.textContent = '';
                    nameEl.textContent = data.naziv || data.Naziv || data.sifra || data.Sifra || '—';
                    var cena = (data.redovnaCena ?? data.RedovnaCena) || null;
                    if (cena === null || cena === undefined) {
                        regularPrice = null;
                        priceEl.textContent = 'Cena: nije dostupna';
                    } else {
                        regularPrice = Number(cena);
                        priceEl.textContent = 'Redovna cena: ' + regularPrice.toFixed(2) + ' RSD';

                        var akcVal = akcijskaInput && akcijskaInput.value ? String(akcijskaInput.value).trim() : '';
                        var discVal = discountInput && discountInput.value ? String(discountInput.value).trim() : '';

                        if (akcijskaInput && akcVal !== '' && Number(akcVal) > 0) {
                            computePercentFromPrice(akcVal);
                        } else if (discountInput && discVal !== '') {
                            computePriceFromPercent(discVal);
                        } else {
                            if (discountInput) discountInput.value = '0.00';
                        }
                    }
                    return true;
                })
                .catch(function (err) {
                    console.error('[Akcija] fetch error', err);
                    articleExists = false;
                    if (clientErrorEl) clientErrorEl.textContent = 'Greška pri učitavanju artikla.';
                    nameEl.textContent = 'Greška pri učitavanju: ' + (err && err.message ? err.message : '');
                    priceEl.textContent = '';
                    return false;
                });
        }

        function fetchSuggestions(prefix) {
            if (!prefix) { hideSuggestions(); return; }
            prefix = prefix.trim().toUpperCase();
            var url = '/Akcija/LookupArtikli?prefix=' + encodeURIComponent(prefix);

            fetch(url, { credentials: 'same-origin' })
                .then(function (resp) { if (!resp.ok) return []; return resp.json(); })
                .then(function (items) { renderSuggestions(items || []); })
                .catch(function (err) { console.error('[Akcija] suggestions error', err); hideSuggestions(); });
        }

        function renderSuggestions(items) {
            if (!suggestionsEl) return;
            suggestionsEl.innerHTML = '';
            if (!items || items.length === 0) { hideSuggestions(); return; }

            items.forEach(function (it) {
                var label = document.createElement('a');
                label.href = '#';
                label.className = 'list-group-item list-group-item-action';
                var name = it.naziv || it.Naziv || '';
                var sifra = it.sifra || it.Sifra || '';
                var cena = (it.redovnaCena ?? it.RedovnaCena) || null;
                label.innerHTML = '<div style="display:flex;justify-content:space-between;gap:12px;">' +
                    '<div><strong>' + escapeHtml(sifra) + '</strong> <span style="color:#6b7571">' + escapeHtml(name) + '</span></div>' +
                    '<div style="white-space:nowrap;color:#4b5650">' + (cena ? Number(cena).toFixed(2) + ' RSD' : '') + '</div>' +
                    '</div>';

                label.addEventListener('mousedown', function (ev) { ev.preventDefault(); });
                label.addEventListener('click', function (ev) {
                    ev.preventDefault();
                    input.value = sifra;
                    hideSuggestions();
                    fetchArticle(sifra);
                });

                suggestionsEl.appendChild(label);
            });

            suggestionsEl.style.display = 'block';
        }

        function hideSuggestions() { if (suggestionsEl) suggestionsEl.style.display = 'none'; }

        function ensureTipovi() {
            if (!tipSelect) return;
            // if only default option present, fetch tipovi
            if (tipSelect.options.length > 1) return;

            fetch('/Akcija/Tipovi', { credentials: 'same-origin' })
                .then(function (r) { if (!r.ok) return []; return r.json(); })
                .then(function (items) {
                    if (!items || items.length === 0) return;
                    items.forEach(function (t) {
                        var opt = document.createElement('option');
                        opt.value = t.tipAkcijeId || t.TipAkcijeId || '';
                        opt.textContent = t.naziv || t.Naziv || '';
                        tipSelect.appendChild(opt);
                    });
                })
                .catch(function (e) { console.error('Failed to load tipovi', e); });
        }

        function escapeHtml(s) { return String(s || '').replace(/[&<>\"]/g, function (m) { return ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;'}[m]); }); }

        function computePercentFromPrice(priceVal) {
            if (ignoreCalc) return;
            if (!regularPrice || regularPrice <= 0) return;
            var p = Number(String(priceVal).replace(',', '.'));
            if (isNaN(p)) return;
            var percent = (1 - (p / regularPrice)) * 100;
            ignoreCalc = true;
            if (discountInput) discountInput.value = percent.toFixed(2);
            ignoreCalc = false;
        }

        function computePriceFromPercent(percentVal) {
            if (ignoreCalc) return;
            if (!regularPrice || regularPrice <= 0) return;
            var pct = Number(String(percentVal).replace(',', '.'));
            if (isNaN(pct)) return;
            var price = regularPrice * (1 - (pct / 100));
            ignoreCalc = true;
            if (akcijskaInput) akcijskaInput.value = price.toFixed(2);
            ignoreCalc = false;
        }

        input.addEventListener('input', function () {
            if (timeout) clearTimeout(timeout);
            timeout = setTimeout(function () {
                var val = input.value.trim();
                if (val.length === 0) { clearPreview(); hideSuggestions(); return; }
                fetchSuggestions(val);
            }, 300);
        });

        input.addEventListener('blur', function () {
            if (timeout) clearTimeout(timeout);
            setTimeout(function () {
                var val = input.value.trim();
                hideSuggestions();
                if (val.length > 0) fetchArticle(val);
            }, 150);
        });

        // prevent form submit on Enter when article doesn't exist
        input.addEventListener('keydown', function (e) {
            if (e.key === 'Enter') {
                e.preventDefault();
                var val = input.value.trim();
                if (val.length === 0) return;
                fetchArticle(val).then(function (ok) {
                    if (!ok) {
                        input.focus();
                    }
                });
            }
        });

        var form = input.closest('form');
        if (form) {
            form.addEventListener('submit', function (e) {
                var val = input.value.trim();
                if (!val) return; // leave validation to server

                if (articleExists === true) return; // ok

                e.preventDefault();
                fetchArticle(val).then(function (ok) {
                    if (ok) {
                        form.submit();
                    } else {
                        input.focus();
                    }
                });
            });
        }

        if (akcijskaInput) {
            akcijskaInput.addEventListener('input', function () {
                if (ignoreCalc) return;
                var v = akcijskaInput.value.trim();
                if (v === '') return;
                computePercentFromPrice(v);
            });
        }

        if (discountInput) {
            discountInput.addEventListener('input', function () {
                if (ignoreCalc) return;
                var v = discountInput.value.trim();
                if (v === '') return;
                computePriceFromPercent(v);
            });
        }

    })();
});
