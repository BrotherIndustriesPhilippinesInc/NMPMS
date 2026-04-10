// ================= ANALYSIS PHOTO UPLOAD =================
const analysisPhotoDrop = document.getElementById('photoDrop_analysis');
const analysisPhotoInput = document.getElementById('cause_photo');

let analysisFiles = [];

// Render preview
function renderAnalysisPreview() {
    analysisPhotoDrop.innerHTML = '';

    if (analysisFiles.length === 0) {
        analysisPhotoDrop.innerHTML = '<div class="photo-preview-placeholder">Drag & Drop or Click to Upload</div>';
        return;
    }

    const grid = document.createElement('div');
    grid.className = 'photo-preview-grid';

    analysisFiles.forEach((file, index) => {
        const item = document.createElement('div');
        item.className = 'preview-item';

        const removeBtn = document.createElement('button');
        removeBtn.innerHTML = '×';
        removeBtn.className = 'remove-btn';

        removeBtn.onclick = (e) => {
            e.stopPropagation();
            analysisFiles.splice(index, 1);
            renderAnalysisPreview();
        };

        const reader = new FileReader();

        reader.onload = (e) => {
            let element;

            if (file.type.startsWith('image/')) {
                element = document.createElement('img');
                element.src = e.target.result;
            } else if (file.type.startsWith('video/')) {
                element = document.createElement('video');
                element.src = e.target.result;
                element.controls = true;
            }

            item.appendChild(element);
            item.appendChild(removeBtn);
        };

        reader.readAsDataURL(file);
        grid.appendChild(item);
    });

    analysisPhotoDrop.appendChild(grid);
}

// Click
analysisPhotoDrop.addEventListener('click', () => analysisPhotoInput.click());

// Change
analysisPhotoInput.addEventListener('change', (e) => {
    handleAnalysisFiles(e.target.files);
});

// Drag
analysisPhotoDrop.addEventListener('dragover', (e) => {
    e.preventDefault();
    analysisPhotoDrop.classList.add('dragover');
});

analysisPhotoDrop.addEventListener('dragleave', () => {
    analysisPhotoDrop.classList.remove('dragover');
});

analysisPhotoDrop.addEventListener('drop', (e) => {
    e.preventDefault();
    analysisPhotoDrop.classList.remove('dragover');
    handleAnalysisFiles(e.dataTransfer.files);
});

// Handle files
function handleAnalysisFiles(files) {
    for (let file of files) {

        // prevent duplicates
        if (analysisFiles.some(f => f.name === file.name && f.size === file.size)) {
            continue;
        }

        analysisFiles.push(file);
    }

    renderAnalysisPreview();
}

document.getElementById('pmlModal').addEventListener('hidden.bs.modal', () => {
    analysisFiles = [];
    renderAnalysisPreview();
    analysisPhotoInput.value = '';
});

renderAnalysisPreview();




const container = document.getElementById('scrollArea');
const sections = document.querySelectorAll('.section');
const buttons = document.querySelectorAll('.step-btn');
const bar = document.getElementById('progressBar');

let currentStep = 1;

function jump(id, btn) {
    document.getElementById(id).scrollIntoView({ behavior: 'smooth' });
    buttons.forEach(b => b.classList.remove('active'));
    btn.classList.add('active');
    currentStep = parseInt(id.replace("s", ""));
}

container.addEventListener('scroll', () => {
    let index = 0;
    sections.forEach((s, i) => {
        if (container.scrollTop >= s.offsetTop - 200) index = i;
    });
    buttons.forEach(b => b.classList.remove('active'));
    buttons[index]?.classList.add('active');
    bar.style.width = ((index + 1) / sections.length * 100) + '%';
});

document.getElementById("saveBtn").addEventListener("click", function () {

    const formData = new FormData();

    formData.append("step", currentStep);
    formData.append("control_no", document.getElementById("control_no_input").value);

    if (currentStep === 1) {
        formData.append("progress", document.getElementById("progress").value);
        formData.append("pms_name", document.getElementById("pms_name").value);
        formData.append("replace_attachment", document.getElementById("problem_category").value);
        formData.append("replace_photo", document.getElementById("replace_photo").value);
        formData.append("pms_area", document.getElementById("pms_area").value);
        formData.append("pms_process", document.getElementById("pms_process").value);
        formData.append("pms_issued_by", document.getElementById("pms_issued_by").value);
    }

    else if (currentStep === 2) {

        // if(control_no = ""){
        //     alert("no Control no");
        //     return;
        // }

        formData.append("analysis_cause", document.getElementById("analysis_cause").value);
        formData.append("defect_details", document.getElementById("defect_details").value);
        formData.append("problem_category", document.getElementById("problem_category").value);
        formData.append("analysis_by", document.getElementById("analysis_by").value);
        formData.append("finish_date", document.getElementById("finish_date").value);
        formData.append("problem_rank", document.getElementById("problem_rank").value);

        const attachment = document.getElementById("defect_attachment").files[0];
        if (attachment) formData.append("defect_attachment", attachment);

        // const photo = document.getElementById("cause_photo").files[0];
        // if(photo) formData.append("cause_photo", photo);
        analysisFiles.forEach(file => {
            formData.append("cause_photo[]", file);
        });
    }

    else if (currentStep == 3) {

        formData.append("analysis_cause", document.getElementById("analysis_cause").value);
        formData.append("defect_details", document.getElementById("defect_details").value);
        formData.append("problem_category", document.getElementById("problem_category").value);
        formData.append("analysis_by", document.getElementById("analysis_by").value);
        formData.append("finish_date", document.getElementById("finish_date").value);
        formData.append("problem_rank", document.getElementById("problem_rank").value);

        const attachment = document.getElementById("defect_attachment").files[0];
        if (attachment) formData.append("defect_attachment", attachment);


        // Immediate Action
        formData.append("assembly", document.getElementById("assembly").value);
        formData.append("parts", document.getElementById("parts").value);
        formData.append("machine", document.getElementById("machine").value);
        formData.append("system", document.getElementById("system").value);
        formData.append("detail_action_by", document.getElementById("detail_action_by").value);

        const detailFile = document.getElementById("detail_attachment").files[0];
        if (detailFile) formData.append("detail_attachment", detailFile);

        formData.append("fg_treatment", document.getElementById("fg_treatment").value);
        formData.append("process_change", document.getElementById("process_change").value);
        formData.append("wi_change", document.getElementById("wi_change").value);
        formData.append("re_education", document.getElementById("re_education").value);
        formData.append("change_manpower", document.getElementById("change_manpower").value);
        formData.append("other_action", document.getElementById("other_action").value);
        formData.append("action_by", document.getElementById("action_by").value);
        formData.append("action_date", document.getElementById("action_date").value);

        formData.append("parts_sorting", document.getElementById("parts_sorting").value);
        formData.append("sorting_result", document.getElementById("sorting_result").value);
        formData.append("enough_stocks_qty", document.getElementById("enough_stocks_qty").value);
        formData.append("proceed_trial", document.getElementById("proceed_trial").value);
        formData.append("trial_reason", document.getElementById("trial_reason").value);
        formData.append("parts_action_by", document.getElementById("parts_action_by").value);
        formData.append("parts_action_date", document.getElementById("parts_action_date").value);

    }

    else if (currentStep == 4) {

        formData.append("s4_assembly", document.getElementById("s4_assembly").value);
        formData.append("s4_parts", document.getElementById("s4_parts").value);
        formData.append("s4_machine", document.getElementById("s4_machine").value);
        formData.append("s4_system", document.getElementById("s4_system").value);
        formData.append("s4_detail_action_by", document.getElementById("s4_detail_action_by").value);
        formData.append("implematation_Date", document.getElementById("implematation_Date").value);
        const s4_detail_attachment = document.getElementById("s4_detail_attachment").files[0];
        if (s4_detail_attachment) formData.append("s4_detail_attachment", s4_detail_attachment);
    }

    else if (currentStep == 5) {
        formData.append("s5_assembly", document.getElementById("s5_assembly").value);
        formData.append("s5_parts", document.getElementById("s5_parts").value);
        formData.append("s5_machine", document.getElementById("s5_machine").value);
        formData.append("s5_system", document.getElementById("s5_system").value);
        formData.append("s5_pic", document.getElementById("s5_pic").value);
        formData.append("s5_implematation_Date", document.getElementById("s5_implematation_Date").value);
    }

    else if (currentStep == 6) {
        formData.append("s6_assembly", document.getElementById("s6_assembly").value);
        formData.append("s6_parts", document.getElementById("s6_parts").value);
        formData.append("s6_machine", document.getElementById("s6_machine").value);
        formData.append("s6_system", document.getElementById("s6_system").value);
        formData.append("ishorizontal", document.getElementById("ishorizontal").value);
        formData.append("s6_model", document.getElementById("s6_model").value);
        formData.append("s6_implematation_Date", document.getElementById("s6_implematation_Date").value);
    }

    else if (currentStep == 7) {
        formData.append("s7_action_judgement", document.getElementById("s7_action_judgement").value);
        formData.append("s7_action_no", document.getElementById("s7_action_no").value);
        formData.append("s7_rank", document.getElementById("s7_rank").value);
        formData.append("s7_pic", document.getElementById("s7_pic").value);
    }




    fetch("/PMS/SaveStep", {
        method: "POST",
        body: formData
    })
        .then(res => res.json())
        .then(data => {

            if (data.message == "Invalid step") {
                swal.fire({
                    icon: "info",
                    title: data.message,
                    text: "Make sure to select a steps!"
                })
            } else {
                swal.fire({
                    icon: "success",
                    title: data.message
                })
            }
        });

});

const modal = document.getElementById('pmlModal');
const editBtn = document.getElementById('editBtn');
const cancelBtn = document.getElementById('cancelEditBtn');

// function setViewMode() {
//     modal.classList.add('view-mode');

//     modal.querySelectorAll('input, textarea, select').forEach(el => {
//         el.setAttribute('readonly', true);
//         el.setAttribute('disabled', true);
//     });

//     editBtn.classList.remove('d-none');
//     cancelBtn.classList.add('d-none');
// }

// function setEditMode() {
//     modal.classList.remove('view-mode');

//     modal.querySelectorAll('input, textarea, select').forEach(el => {
//         el.removeAttribute('readonly');
//         el.removeAttribute('disabled');
//     });

//     editBtn.classList.add('d-none');
//     cancelBtn.classList.remove('d-none');
// }

// function setViewMode() {
//     modal.classList.add('view-mode');

//     modal.querySelectorAll('input, textarea, select').forEach(el => {
//         el.setAttribute('readonly', true);
//         el.setAttribute('disabled', true);
//     });

//     document.getElementById("photoPreviewContainer1").classList.remove("d-none");
//     document.getElementById("photoDrop_analysis").classList.add("d-none");

//     editBtn.classList.remove('d-none');
//     cancelBtn.classList.add('d-none');
// }
function setViewMode() {
    modal.classList.add('view-mode');

    modal.querySelectorAll('input, textarea, select').forEach(el => {
        el.setAttribute('readonly', true);
        el.setAttribute('disabled', true);
    });

    // SHOW gallery
    document.getElementById("photoPreviewContainer1").classList.remove("d-none");

    // HIDE upload
    document.getElementById("photoDrop_analysis").classList.add("d-none");

    // 🔥 SCROLL TO TOP (important for report feel)
    document.getElementById("scrollArea").scrollTop = 0;

    editBtn.classList.remove('d-none');
    cancelBtn.classList.add('d-none');
}

function setEditMode() {
    modal.classList.remove('view-mode');

    modal.querySelectorAll('input, textarea, select').forEach(el => {
        el.removeAttribute('readonly');
        el.removeAttribute('disabled');
    });

    // 🔥 SHOW upload, HIDE gallery
    document.getElementById("photoPreviewContainer1").classList.add("d-none");
    document.getElementById("photoDrop_analysis").classList.remove("d-none");

    editBtn.classList.add('d-none');
    cancelBtn.classList.remove('d-none');
}

modal.addEventListener('shown.bs.modal', () => {
    setViewMode();
});

editBtn.addEventListener('click', () => {
    setEditMode();
});

cancelBtn.addEventListener('click', () => {
    setViewMode();
});

document.getElementById("saveBtn").addEventListener("click", function () {
    setTimeout(() => {
        setViewMode();
    }, 500);
});