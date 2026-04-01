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

    fetch(`/Home/fetch_pml?stage=${encodeURIComponent(stage)}&model=${encodeURIComponent(model)}`)
        .then(response => response.json())
        .then(data => {
            console.log("Filtered Data:", data);
            pml_listTable(data);
           
        })
        .catch(error => console.error('Error fetching data:', error));
});


function pml_listTable(data) {
    updateStatCards(data);

    if ($.fn.DataTable.isDataTable('#tbl_pml')) {
        $('#tbl_pml').DataTable().clear().destroy();
    }

    const tableBody = document.getElementById("tbody_pml");
    tableBody.innerHTML = "";

    const fragment = document.createDocumentFragment();

    if (!data || data.length === 0) {
        tableBody.innerHTML = `<tbody id="tbody_pml">
            <tr id="noDataRow">
                <td colspan="16" class="text-center text-muted">
                    No selected data yet
                </td>
            </tr>
        </tbody>`;
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

    const table = $('#tbl_pml').DataTable({
        responsive: true,
        autoWidth: false,
        deferRender: true,
        pageLength: 10
    });

    $('#tbl_pml tbody').off('dblclick').on('dblclick', 'tr', function () {
        const rowData = JSON.parse(this.dataset.rowData);
        openPmlModal(rowData);
    });

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
            }
            else {
                $(this).find('.count').text(counts[status] || 0);
            }

        });
    }
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
  $('#progress').text(item.pms_create);
  $('#time').text(item.issued_date);
  $('#pic').text(item.person_incharge);
  $('#title').text(item.problem_name);

  $('#pms_part_code').text(item.part_code);
  $('#pms_part_name').text(item.part_name);
  $('#pms_supplier_name').text(item.supplier);

  $('#pms_name').val(item.problem_name);
  $('#pms_details').val(item.phenomenon_details);

  $('#pms_stage').val(item.stage);
  $('#pms_model').val(item.model);
  $('#pms_serial').val(item.serial_number);
  $('#pms_area').val(item.area_detection);
  $('#pms_process').val(item.process);
  $('#pms_issued_by').val(item.issued_by);
  // $('#control_no').text(control_no);
  $('#pms_control_no').text(control_no);
  $('#control_no_input').val(control_no);

    if (item.problem_photo) {
        $('#photoPreviewContainer').html(`
        <img src="upload/PhotoUpload/${item.problem_photo}" 
             class="img-fluid rounded shadow-sm mb-2"
             style="max-height:200px;">
        <div>
            <small class="text-muted">Click image to enlarge</small>
        </div>
    `);
    } else {
        $('#photoPreviewContainer').html('<small class="text-muted">No Photo Uploaded</small>');
    }

    if (item.attachment_name) {

        // Show filename preview inside modal
        $('#attachmentPreviewContainer').html(`
        <div class="mt-1 text-muted small">
            ${item.attachment_name}
        </div>
        <button class="btn btn-sm btn-primary mt-2" 
                onclick="viewAttachment('${item.attachment_name}','1')">
            View Attachment
        </button>
    `);

    } else {
        $('#attachmentPreviewContainer').html('<small class="text-muted">No Attachment</small>');
    }

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

function viewAttachment(filename, step) {

    const folderName = FILE_FOLDERS[step] || 'AttachmentFile';
    const fileUrl = `upload/${folderName}/${filename}`;
    const ext = filename.split('.').pop().toLowerCase();

    let content = '';

    if (['jpg', 'jpeg', 'png', 'gif', 'webp'].includes(ext)) {
        content = `<img src="${fileUrl}" style="max-width:100%; max-height:80vh;">`;
    } else {
        content = `<iframe src="${fileUrl}" style="width:100%; height:75vh; border:none;"></iframe>`;
    }

    Swal.fire({
        title: filename,
        html: content,
        width: '95%',
        showCloseButton: true,
        showConfirmButton: false
    });
}
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


const photoDrop = document.getElementById('photoDrop');
const photoInput = document.getElementById('problem_photos');

photoDrop.addEventListener('click', () => photoInput.click());

photoDrop.addEventListener('dragover', e => {
  e.preventDefault();
  photoDrop.classList.add('border-primary');
});

photoDrop.addEventListener('dragleave', () => {
  photoDrop.classList.remove('border-primary');
});

photoDrop.addEventListener('drop', e => {
  e.preventDefault();
  photoDrop.classList.remove('border-primary');
  photoInput.files = e.dataTransfer.files;
  previewPhotos(photoInput.files);
});

photoInput.addEventListener('change', () => {
  previewPhotos(photoInput.files);
});

function previewPhotos(files) {
  photoDrop.innerHTML = '';

  if (!files.length) {
    photoDrop.innerHTML = '<div class="photo-preview-placeholder">Drag & Drop or Click to Upload</div>';
    return;
  }

  const grid = document.createElement('div');
  grid.className = 'photo-preview-grid';

  Array.from(files).forEach(file => {
    if (!file.type.startsWith('image/')) return;

    const reader = new FileReader();
    reader.onload = e => {
      const img = document.createElement('img');
      img.src = e.target.result;
      grid.appendChild(img);
    };
    reader.readAsDataURL(file);
  });

  photoDrop.appendChild(grid);
}

document.getElementById('pmsModal').addEventListener('hidden.bs.modal', () => {
  photoDrop.innerHTML = '<div class="photo-preview-placeholder">Drag & Drop or Click to Upload</div>';
  photoInput.value = '';
});



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


function savePMS() {
    const form = document.getElementById('pmsForm');
    const formData = new FormData(form);

    fetch('/Home/CreateIssue', {
        method: 'POST',
        body: formData
    })
    .then(res => res.json())
    .then(data => {
        if (data.status === 'success') {
            swal.fire({
                icon: 'success',
                title: 'Created Successfully',
                text: 'Control No.: '+ data.control_no

            })
            bootstrap.Modal.getInstance(document.getElementById('pmsModal')).hide();
            form.reset();

            const model = document.getElementById('modelInput').value;
            const stage = document.getElementById('stageInput').value;

            if (model && stage) {
                 fetch(`/Home/fetch_pml?stage=${encodeURIComponent(stage)}&model=${encodeURIComponent(model)}`)
                 .then(response => response.json())
                 .then(data => {
                        console.log("Reloaded Data:", data);
                        pml_listTable(data);
                 })
                 .catch(error => console.error('Error reloading table:', error));
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