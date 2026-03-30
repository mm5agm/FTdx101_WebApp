fetch('/api/calibration/all')
    .then(r => r.json())
    .then(data => {
        const app = document.getElementById('calibration-app');
        app.innerHTML = `<pre>${JSON.stringify(data, null, 2)}</pre>`;
    })
    .catch(err => {
        document.getElementById('calibration-app').innerText =
            "Failed to load calibration tables.";
    });
