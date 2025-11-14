// wwwroot/js/loehne-tab.js

(function () {
    const COMPANY_ID = window.COMPANY_ID;
    const API_BASE = (window.API_BASE_URL || '').replace(/\/+$/, '');

    console.log('[loehne] COMPANY_ID =', COMPANY_ID);
    console.log('[loehne] API_BASE =', API_BASE);

    if (!COMPANY_ID) {
        console.warn('[loehne] Kein COMPANY_ID gesetzt – Abbruch.');
        return;
    }

    function api(path) {
        // path: "/api/xyz"
        return API_BASE + path;
    }

    function formatMoney(val) {
        if (val === null || val === undefined) return '0.00';
        return Number(val).toFixed(2);
    }

    function showToast(type, msg) {
        if (window.toastr) {
            if (type === 'error') toastr.error(msg);
            else if (type === 'success') toastr.success(msg);
            else toastr.info(msg);
        } else {
            alert(msg);
        }
    }

    // ---------------------------
    // 1) Mitarbeiterları modal için yükle
    // ---------------------------
    async function loadEmployeesForModal() {
        const select = document.getElementById('employeeId');
        if (!select) return;

        // Zaten yüklenmişse tekrar çağırma (isteğe bağlı)
        if (select.options.length > 1) {
            return;
        }

        const url = api(`/api/Company/${COMPANY_ID}/Employees`);
        console.log('[loehne] GET employees', url);

        try {
            const res = await fetch(url);
            if (!res.ok) {
                console.error('[loehne] Mitarbeiter Laden HTTP-Fehler', res.status);
                showToast('error', 'Mitarbeiter konnten nicht geladen werden.');
                return;
            }
            const json = await res.json(); // ApiResponse<IEnumerable<EmployeeDto>>
            console.log('[loehne] employees response', json);

            if (!json.success || !Array.isArray(json.data)) {
                showToast('error', json.message || 'Fehler beim Laden der Mitarbeiter.');
                return;
            }

            // Temizle & default option ekle
            select.innerHTML = '';
            const optDefault = document.createElement('option');
            optDefault.value = '';
            optDefault.textContent = '-- bitte wählen --';
            select.appendChild(optDefault);

            json.data.forEach(e => {
                const opt = document.createElement('option');
                opt.value = e.id;
                opt.textContent = `${e.firstName} ${e.lastName}`;
                select.appendChild(opt);
            });

            if (json.data.length === 0) {
                showToast('info', 'Keine Mitarbeiter für diese Firma vorhanden.');
            }
        } catch (err) {
            console.error('[loehne] Fehler beim Laden der Mitarbeiter:', err);
            showToast('error', 'Fehler beim Laden der Mitarbeiter (Netzwerk).');
        }
    }

    // ---------------------------
    // 2) Firma için aylık Löhne listesini yükle
    // ---------------------------
    async function loadMonthlyLohns() {
        const tbody = document.querySelector('#tblLohnMonthly tbody');
        const periodInput = document.getElementById('period');

        if (!tbody) return;

        let period = periodInput && periodInput.value ? periodInput.value : null;

        // Period boşsa: bugünün ayını kullan
        if (!period) {
            const now = new Date();
            const y = now.getFullYear();
            const m = String(now.getMonth() + 1).padStart(2, '0');
            period = `${y}-${m}`;
            if (periodInput) periodInput.value = period;
        }

        const url = api(`/api/Lohn/by-company/${COMPANY_ID}/monthly?period=${encodeURIComponent(period)}`);
        console.log('[loehne] GET', url);

        tbody.innerHTML = `
            <tr>
                <td colspan="6" class="text-center text-muted">Lade Daten...</td>
            </tr>`;

        try {
            const res = await fetch(url);
            if (!res.ok) {
                console.error('[loehne] Löhne HTTP-Fehler', res.status);
                tbody.innerHTML = `
                    <tr>
                        <td colspan="6" class="text-center text-danger">Fehler beim Laden der Löhne (HTTP ${res.status}).</td>
                    </tr>`;
                return;
            }

            const json = await res.json(); // ApiResponse<IEnumerable<LohnMonthlyRowDto>>
            console.log('[loehne] monthly loehne response', json);

            if (!json.success) {
                tbody.innerHTML = `
                    <tr>
                        <td colspan="6" class="text-center text-danger">${json.message || 'Fehler beim Laden der Löhne.'}</td>
                    </tr>`;
                return;
            }

            const rows = json.data || [];
            if (rows.length === 0) {
                tbody.innerHTML = `
                    <tr>
                        <td colspan="6" class="text-center text-muted">Keine Löhne für diesen Monat gefunden.</td>
                    </tr>`;
                return;
            }

            tbody.innerHTML = '';

            rows.forEach(row => {
                const tr = document.createElement('tr');

                const colEmp = document.createElement('td');
                colEmp.textContent = row.employeeName || '';
                tr.appendChild(colEmp);

                const colMonth = document.createElement('td');
                colMonth.textContent = `${String(row.month).padStart(2, '0')}.${row.year}`;
                tr.appendChild(colMonth);

                const colBrutto = document.createElement('td');
                colBrutto.textContent = formatMoney(row.bruttoSalary);
                tr.appendChild(colBrutto);

                const colNet = document.createElement('td');
                colNet.textContent = formatMoney(row.netSalary);
                tr.appendChild(colNet);

                const colStatus = document.createElement('td');
                // Şimdilik hepsi Entwurf; ileride IsFinal geldiğinde güncelleriz
                colStatus.innerHTML = `<span class="badge badge-secondary">Entwurf</span>`;
                tr.appendChild(colStatus);

                const colActions = document.createElement('td');
                colActions.innerHTML = `
                    <a class="btn btn-sm btn-outline-info"
                       href="/Employees/Details/${row.employeeId}#lohnverlauf">
                        <i class="fas fa-eye"></i> Verlauf
                    </a>`;
                tr.appendChild(colActions);

                tbody.appendChild(tr);
            });

        } catch (err) {
            console.error('[loehne] Fehler beim Laden der Löhne:', err);
            tbody.innerHTML = `
                <tr>
                    <td colspan="6" class="text-center text-danger">Fehler beim Laden der Löhne (Netzwerk).</td>
                </tr>`;
        }
    }

    // ---------------------------
    // 3) Lohn berechnen + speichern (calc-and-save)
    // ---------------------------
    async function doCalcAndSave() {
        const selEmp = document.getElementById('employeeId');
        const inpMonth = document.getElementById('calcMonth');

        if (!selEmp || !inpMonth) return;

        const empId = parseInt(selEmp.value, 10);
        const monthVal = inpMonth.value; // "YYYY-MM"

        if (!empId) {
            showToast('error', 'Bitte Mitarbeiter auswählen.');
            return;
        }
        if (!monthVal) {
            showToast('error', 'Bitte Monat auswählen.');
            return;
        }

        // period DateTime string: "YYYY-MM-01T00:00:00"
        const periodIso = `${monthVal}-01T00:00:00`;

        const payload = {
            employeeId: empId,
            period: periodIso
            // Diğer alanlar server tarafında Employee’den override ediliyor.
        };

        const url = api('/api/Lohn/calc-and-save');
        console.log('[loehne] POST calc-and-save', url, payload);

        try {
            const res = await fetch(url, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(payload)
            });

            const json = await res.json();
            console.log('[loehne] calc-and-save response', json);

            if (!res.ok || !json.success) {
                showToast('error', json.message || 'Lohnberechnung fehlgeschlagen.');
                return;
            }

            showToast('success', json.message || 'Lohnberechnung erfolgreich.');

            // Modal kapat
            if (typeof $ !== 'undefined') {
                $('#calcModal').modal('hide');
            }

            // Listeyi seçilen ay için yeniden yükle
            const periodInput = document.getElementById('period');
            if (periodInput && !periodInput.value) {
                periodInput.value = monthVal;
            }
            await loadMonthlyLohns();

        } catch (err) {
            console.error('[loehne] Fehler bei calc-and-save:', err);
            showToast('error', 'Fehler bei der Berechnung (Netzwerk).');
        }
    }

    // ---------------------------
    // 4) Event binding
    // ---------------------------
    function init() {
        // "Löhne" tabı açıldığında ilk yükleme
        if (typeof $ !== 'undefined' && $.fn && $.fn.tab) {
            $('a[href="#loehne"]').on('shown.bs.tab', function () {
                loadMonthlyLohns();
            });
        }

        // Sayfa zaten Löhne tab ile geliyorsa (hash vs.) yine de yüklemeyi deneyelim:
        const loehneTab = document.getElementById('loehne');
        if (loehneTab && loehneTab.classList.contains('show') && loehneTab.classList.contains('active')) {
            loadMonthlyLohns();
        }

        const btnLoad = document.getElementById('btnLoadLohns');
        if (btnLoad) {
            btnLoad.addEventListener('click', function (e) {
                e.preventDefault();
                loadMonthlyLohns();
            });
        }

        const btnDoCalc = document.getElementById('btnDoCalc');
        if (btnDoCalc) {
            btnDoCalc.addEventListener('click', function (e) {
                e.preventDefault();
                doCalcAndSave();
            });
        }

        // Modal açıldığında Mitarbeiter listesini yükle
        if (typeof $ !== 'undefined') {
            $('#calcModal').on('shown.bs.modal', function () {
                loadEmployeesForModal();
            });
        }
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();
