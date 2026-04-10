//function get_host() {
//    const hostname = window.location.hostname;


//    console.log(hostname);

//    if (hostname === 'localhost' || hostname === '127.0.0.1') {
//        return 'NMPMS/';
//    } else if (hostname === 'apbiphbpswb01') {
//        return 'NMPMS/';
//    } else {
//        console.warn('Unknown host. Defaulting to localhost.');
//        return 'NMPMS/';
//    }

//}

//fetch('/Home/fetch_pml')
//  .then(response => response.json())
//  .then(data => {
//    console.log("Received Data:", data);
//    pml_listTable(data);
//  })
//  .catch(error => console.error('Error fetching data:', error));

$(document).ready(function () {

    // ✅ Load ALL data on page load (no filter)
    loadPmlData();

    // ✅ Activate button (with filter)
    $('#activateBtn').on('click', function () {

        const model = $('.model-select').val();
        const stage = $('.stage-select').val();

        if (!model || !stage) {
            Swal.fire('Missing Selection', 'Please select both Model and Stage.', 'warning');
            return;
        }

        document.getElementById('modelInput').value = model;
        document.getElementById('stageInput').value = stage;

        $('#thModel').text(model);
        $('#thStage').text(stage);

        $('#createIssueBtn').removeClass('d-none');

        // ✅ Load filtered data
        loadPmlData(stage, model);
    });

});

function loadPmlData(stage = "", model = "") {

    let url = `/Home/fetch_pml`;

    if (stage && model) {
        url += `?stage=${encodeURIComponent(stage)}&model=${encodeURIComponent(model)}`;
    }

    fetch(url)
        .then(response => response.json())
        .then(data => {
            console.log("PML Data:", data);
            pml_listTable(data);
        })
        .catch(error => console.error('Error fetching PML:', error));
    let chartUrl = `/Home/fetch_graph`;

    if (stage && model) {
        chartUrl += `?stage=${encodeURIComponent(stage)}&model=${encodeURIComponent(model)}`;
    }

    fetch(chartUrl)
        .then(response => response.json())
        .then(data => {
            console.log("Chart Data:", data);
            renderProblemCategoryChart(data);
        })
        .catch(error => console.error('Error fetching chart:', error));
}


/* ============================
   ✅ TABLE RENDER
============================ */
function pml_listTable(data) {

    updateStatCards(data);

    // ✅ Destroy existing table safely
    if ($.fn.DataTable.isDataTable('#tbl_pml')) {
        $('#tbl_pml').DataTable().clear().destroy();
    }

    const tableBody = document.getElementById("tbody_pml");
    tableBody.innerHTML = "";

    const fragment = document.createDocumentFragment();

    if (!data || data.length === 0) {
        tableBody.innerHTML = `
            <tr>
                <td colspan="16" class="text-center text-muted">
                    No data available
                </td>
            </tr>`;
        return;
    }

    data.forEach((item, index) => {
        const row = document.createElement("tr");

        row.dataset.rowData = JSON.stringify(item);

        row.innerHTML = `
            <td>${index + 1}</td>
            <td>${item.control_no}</td>
            <td>${item.pms_create}</td>
            <td>${item.person_incharge}</td>
            <td>${item.problem_name}</td>
            <td>${item.phenomenon_details}</td>
            <td>${item.stage}</td>
            <td>${item.model}</td>
            <td>${item.serial_number}</td>
            <td>${item.area_detection}</td>
            <td>${item.process}</td>
            <td>${item.issued_by}</td>
            <td>${item.serial_number}</td>
            <td>${item.issued_date}</td>
            <td>${item.part_name}</td>
            <td>${item.supplier}</td>
        `;

        fragment.appendChild(row);
    });

    tableBody.appendChild(fragment);

    // ✅ Reinitialize DataTable
    $('#tbl_pml').DataTable({
        responsive: true,
        autoWidth: false,
        deferRender: true,
        pageLength: 10
    });

    // ✅ Row double click
    $('#tbl_pml tbody').off('dblclick').on('dblclick', 'tr', function () {
        const rowData = JSON.parse(this.dataset.rowData);
        openPmlModal(rowData);
    });
}

function updateStatCards(data) {

    let counts = {
        "Cause Investigation": 0,
        "Temporary Action": 0,
        "Permanent Action": 0,
        "Closed": 0
    };

    data.forEach(item => {
        if (counts[item.pms_create] !== undefined) {
            counts[item.pms_create]++;
        }
    });

    $('.stat-card').each(function () {
        const status = $(this).data("status");

        if (status === "ALL") {
            $(this).find('.count').text(data.length);
        } else {
            $(this).find('.count').text(counts[status] || 0);
        }
    });
}


let chartInstance = null;

function renderProblemCategoryChart(data) {

    const counts = {};

    data.forEach(item => {
        const category = item.problem_category || "Unknown";
        counts[category] = (counts[category] || 0) + 1;
    });

    const labels = Object.keys(counts);
    const values = Object.values(counts);

    const ctx = document.getElementById('problemCategoryChart').getContext('2d');

    if (chartInstance) {
        chartInstance.destroy();
    }

    chartInstance = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [{
                label: 'Problem Category Count',
                data: values,
                backgroundColor: [
                    '#4e73df',
                    '#1cc88a',
                    '#36b9cc',
                    '#f6c23e',
                    '#e74a3b',
                    '#858796'
                ],
                borderColor: '#333',
                borderWidth: 1
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    labels: {
                        color: '#fff'
                    }
                }
            },
            scales: {
                x: {
                    ticks: { color: '#fff' }
                },
                y: {
                    beginAtZero: true,
                    ticks: { color: '#fff' }
                }
            }
        }
    });
}

$(document).on("click", ".stat-card", function () {
    let status = $(this).data("status");
    let table = $('#tbl_pml').DataTable();

    if (status === "ALL") {
        table.search('').draw();
    }
    else {
        table.column(2).search(status).draw();
    }
});


const FILE_FOLDERS = {
    1: 'AttachmentFile',
    2: 'AnalysisFiles',
    3: 'ImmediateActionFiles',
    4: 'PermanentActionFiles'
};

const PHOTO_FOLDERS = {
    problem: 'PhotoUpload',
    analysis: 'AnalysisPhotos',
    action: 'ActionPhotos'
};
function openPmlModal(item) {
  // $('#control_no').val(item.control_no);
  const control_no = item.control_no;
  $('#progress').val(item.pms_create);
  $('#time').text(item.issued_date);
  $('#pic').text(item.person_incharge);
  $('#title').text(item.problem_name);

  $('#pms_part_code').text(item.part_code);
  $('#pms_part_name').text(item.part_name);
  $('#pms_supplier_name').text(item.supplier);


    loadpheno(control_no);
    loadAnalysis(control_no);
    loadImmediateAction(control_no);
    loadtemp(control_no);
    loadper(control_no);
    loadhorizontal(control_no);
    loadb_action(control_no);

  $('#pmlModal').modal('show');
}

//function viewAttachment(filename, steps) {
//    var folderName = "";
//    if (steps == 1) {
//         folderName = 'AttachmentFile';

//    } else if (steps == 2) {
//         folderName = 'AnalysisFiles';
//    }
//    Swal.fire({
//        title: 'View Attachment',
//        html: `
//            <iframe
//                src="upload/${folderName}/${filename}"
//                style="width:100%; height:70vh; border:none;"
//                loading="lazy">
//            </iframe>
//        `,
//        width: '90%',
//        padding: '0',
//        showCloseButton: true,
//        showConfirmButton: false,
//        allowOutsideClick: true
//    });
//}

//function viewAttachment(filename, step) {

//    const folderName = FILE_FOLDERS[step] || 'AttachmentFile';

//    Swal.fire({
//        title: 'View Attachment',
//        html: `
//            <iframe
//                src="upload/${folderName}/${filename}"
//                style="width:100%; height:75vh; border:none; border-radius:8px;"
//                loading="lazy">
//            </iframe>
//        `,
//        width: '95%',
//        showCloseButton: true,
//        showConfirmButton: false,
//        background: '#fff'
//    });
//}

//function viewAttachment(filename, step) {

//    const folderName = FILE_FOLDERS[step] || 'AttachmentFile';
//    const fileUrl = `upload/${folderName}/${filename}`;
//    const ext = filename.split('.').pop().toLowerCase();

//    let content = '';

//    if (['jpg', 'jpeg', 'png', 'gif', 'webp'].includes(ext)) {
//        content = `<img src="${fileUrl}" style="max-width:100%; max-height:80vh;">`;
//    } else {
//        content = `<iframe src="${fileUrl}" style="width:100%; height:75vh; border:none;"></iframe>`;
//    }

//    Swal.fire({
//        title: filename,
//        html: content,
//        width: '95%',
//        showCloseButton: true,
//        showConfirmButton: false
//    });
//}
function pmsModal() {
  const model = document.getElementById('modelInput').value || '';
  const stage = document.getElementById('stageInput').value || '';

  document.getElementById('selectedModel').value = model || '—';
  document.getElementById('selectedStage').value = stage || '—';

  const now = new Date();
  const dateStr =
    now.getFullYear().toString() +
    String(now.getMonth() + 1).padStart(2, '0') +
    String(now.getDate()).padStart(2, '0') + '-' +
    String(now.getHours()).padStart(2, '0') +
    String(now.getMinutes()).padStart(2, '0') +
    String(now.getSeconds()).padStart(2, '0');

  const controlNo = `${model}-${dateStr}`;

  document.getElementById('control_no').innerText = controlNo;

  document.getElementById('model_hidden').value = model;
  document.getElementById('stage_hidden').value = stage;
  document.getElementById('control_no_hidden').value = controlNo;



  $('#pmsModal').modal('show');
}

// ================= PHOTO UPLOAD (ALIGNED + FIXED) =================
const photoDrop = document.getElementById('photoDrop');
const photoInput = document.getElementById('problem_photos');

let selectedFiles = [];

// Render preview grid
function renderPreview() {
    photoDrop.innerHTML = '';

    if (selectedFiles.length === 0) {
        photoDrop.innerHTML = '<div class="photo-preview-placeholder">Drag & Drop or Click to Upload</div>';
        return;
    }

    const grid = document.createElement('div');
    grid.className = 'photo-preview-grid';

    selectedFiles.forEach((file, index) => {
        const previewItem = document.createElement('div');
        previewItem.className = 'preview-item';

        const removeBtn = document.createElement('button');
        removeBtn.innerHTML = '×';
        removeBtn.className = 'remove-btn';

        removeBtn.onclick = (e) => {
            e.stopPropagation();
            selectedFiles.splice(index, 1);
            renderPreview();
        };

        const reader = new FileReader();

        reader.onload = (e) => {
            let element;

            if (file.type.startsWith('image/')) {
                element = document.createElement('img');
                element.src = e.target.result;
            }
            else if (file.type.startsWith('video/')) {
                element = document.createElement('video');
                element.src = e.target.result;
                element.controls = true;
            }
            else {
                return;
            }

            previewItem.appendChild(element);
            previewItem.appendChild(removeBtn);
        };

        reader.readAsDataURL(file);
        grid.appendChild(previewItem);
    });

    photoDrop.appendChild(grid);
}

photoDrop.addEventListener('click', () => photoInput.click());

photoInput.addEventListener('change', (e) => {
    handleFiles(e.target.files);
});

photoDrop.addEventListener('dragover', (e) => {
    e.preventDefault();
    photoDrop.classList.add('dragover');
});

photoDrop.addEventListener('dragleave', () => {
    photoDrop.classList.remove('dragover');
});

photoDrop.addEventListener('drop', (e) => {
    e.preventDefault();
    photoDrop.classList.remove('dragover');
    handleFiles(e.dataTransfer.files);
});

function handleFiles(files) {
    for (let file of files) {
        if (selectedFiles.some(f => f.name === file.name && f.size === file.size)) {
            continue;
        }

        selectedFiles.push(file);
    }

    renderPreview();
}

document.getElementById('pmsModal').addEventListener('hidden.bs.modal', () => {
    selectedFiles = [];
    renderPreview();
    photoInput.value = '';
});

renderPreview();

function savePMS() {
    const form = document.getElementById('pmsForm');
    const formData = new FormData(form);
    const steps = document.getElementById('stepno').value;
    //selectedFiles.forEach(file => {
    //    formData.append('problem_photos[]', file);
    //});
    formData.append('stepno', steps)

    fetch('/Home/CreateIssue', {
        method: 'POST',
        body: formData
    })
        .then(res => res.json())
        .then(data => {
            if (data.status === 'success') {

                Swal.fire({
                    icon: 'success',
                    title: 'Created Successfully',
                    text: 'Control No.: ' + data.control_no
                });

                bootstrap.Modal.getInstance(document.getElementById('pmsModal')).hide();

                form.reset();

                // reset preview
                selectedFiles = [];
                renderPreview();

                const model = document.getElementById('modelInput').value;
                const stage = document.getElementById('stageInput').value;

                if (model && stage) {
                    fetch(`/Home/fetch_pml?stage=${encodeURIComponent(stage)}&model=${encodeURIComponent(model)}`)
                        .then(res => res.json())
                        .then(data => pml_listTable(data));
                }

            } else {
                alert(data.message);
            }
        })
        .catch(err => {
            console.error(err);
            alert('Saving failed');
        });
}
function previewPhoto(file) {
  const dropArea = document.getElementById('photoDrop');

  if (!file || !file.type.startsWith('image/')) {
    alert('Please select a valid image file');
    return;
  }

  dropArea.innerHTML = '';

  const img = document.createElement('img');
  img.src = URL.createObjectURL(file);
  img.style.height = '110px';
  img.style.maxWidth = '100%';
  img.classList.add('rounded', 'shadow-sm');

  dropArea.appendChild(img);
}
function viewPhoto(filename, type = 'problem') {

    const folder = PHOTO_FOLDERS[type] || 'PhotoUpload';

    Swal.fire({
        title: 'View Photo',
        html: `
            <img 
                src="upload/${folder}/${filename}" 
                style="max-width:100%; max-height:80vh; border-radius:10px;"
            >
        `,
        width: 'auto',
        showCloseButton: true,
        showConfirmButton: false
    });
}
//function savePMS() {

//    phenomenonEditor.save().then((outputData) => {

//        // Convert JSON to string and store in hidden input
//        document.getElementById('phenomenon_details').value =
//            JSON.stringify(outputData);

//        const form = document.getElementById('pmsForm');
//        const formData = new FormData(form);

//        fetch('/Home/CreateIssue', {
//            method: 'POST',
//            body: formData
//        })
//            .then(res => res.json())
//            .then(data => {
//                if (data.status === 'success') {

//                    Swal.fire({
//                        icon: 'success',
//                        title: 'Created Successfully',
//                        text: 'Control No.: ' + data.control_no
//                    });

//                    // Close modal
//                    bootstrap.Modal.getInstance(
//                        document.getElementById('pmsModal')
//                    ).hide();

//                    // Reset form
//                    form.reset();
//                    phenomenonEditor.clear();

//                    // ✅ RELOAD TABLE DATA (same as activateBtn)
//                    const model = document.getElementById('modelInput').value;
//                    const stage = document.getElementById('stageInput').value;

//                    if (model && stage) {
//                        fetch(`/Home/fetch_pml?stage=${encodeURIComponent(stage)}&model=${encodeURIComponent(model)}`)
//                            .then(response => response.json())
//                            .then(data => {
//                                console.log("Reloaded Data:", data);
//                                pml_listTable(data);
//                            })
//                            .catch(error => console.error('Error reloading table:', error));
//                    }

//                } else {
//                    alert(data.message);
//                }
//            })

//    }).catch((error) => {
//        console.error('Editor save failed:', error);
//        alert('Please complete the Phenomenon Details.');
//    });
//}

function populatenew(mName, sName) {
    const $modelSelect = $('.model-select');
    const $stageSelect = $('.stage-select');

    if ($modelSelect.find("option[value='" + mName + "']").length === 0) {
        $modelSelect.append(new Option(mName, mName));
    }

    if ($stageSelect.find("option[value='" + sName + "']").length === 0) {
        $stageSelect.append(new Option(sName, sName));
    }

    $modelSelect.val(mName);
    $stageSelect.val(sName);

    $('#thModel').text(mName);
    $('#thStage').text(sName);

    $('#createIssueBtn').removeClass('d-none');
    $('#activateBtn').trigger('click');
}

$(document).ready(function () {
    $('#createnewBtn').on('click', function () {
        const mName = $('#mName').val();
        const sName = $('#sName').val();

        $.post('Home/createnew', { mName: mName, sName: sName }, function (response) {
            if (response.success) {
                swal.fire({
                    icon: 'success',
                    title: 'Successfully Registered',
                    text: 'Registration completed Successfully'
                }).then(() => {
                    //$('mName').val() = "";
                    //$('sName').val() = "";
                    $('#closeModal').trigger('click');
                    
                    populatenew(mName, sName)
                });
            } else {
                swal.fire({
                    icon: 'error',
                    title: 'Server Error',
                    //text: 'Registration completed Successfully'
                });
                return;

            }
        }).fail(function () {
            swal.fire({
                icon: 'error',
                title: 'Server Error',
                text: 'Something went wrong..'
            })
        })

    });
})