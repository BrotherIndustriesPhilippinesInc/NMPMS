function loadpheno(controlNo) {

    fetch(`/Analysis/GetPheno?control_no=${controlNo}`)
        .then(res => res.json())
        .then(data => {

            if (data.status === "success") {
                const pheno = data.data;
                
                document.getElementById("pms_name").value = pheno.pms_name || "";
                document.getElementById("pms_details").value = pheno.pms_details || "";
                document.getElementById("pms_stage").value = pheno.pms_stage || "";
                document.getElementById("pms_model").value = pheno.pms_model || "";
                document.getElementById("pms_serial").value = pheno.pms_serial || "";
                document.getElementById("pms_area").value = pheno.pms_area || "";
                document.getElementById("pms_process").value = pheno.pms_process || "";
                document.getElementById("pms_issued_by").value = pheno.pms_issued_by || "";
                document.getElementById("control_no_input").value = controlNo || "";
                document.getElementById("pms_partcode").value = pheno.pms_partcode || "";
                document.getElementById("pms_partname").value = pheno.pms_partname || "";
                document.getElementById("pms_supplier").value = pheno.supplier || "";
                //document.getElementById("pic").value = pheno.pic || "";

                

                if (pheno.attachment) {
                    $('#attachmentPreviewContainer').html(`
                        <div class="mt-1 text-muted small">
                            ${pheno.attachment}
                        </div>
                        <button class="btn btn-sm btn-primary mt-2" 
                                onclick="viewAttachment('${pheno.attachment}')">
                            View Attachment
                        </button>
                    `);
                } else {
                    $('#attachmentPreviewContainer').html('<small class="text-muted">No Attachment</small>');
                }

                loadProblemFiles(controlNo, 1, "photoPreviewContainer");


            }
        });
}



function loadProblemFiles(controlNo, step, containerId) {

    fetch(`/Analysis/GetProblemFiles?control_no=${controlNo}&steps=${step}`)
        .then(res => res.json())
        .then(files => {

            if (!files.length) {
                $(`#${containerId}`).html('<small class="text-muted">No Photo Uploaded</small>');
                return;
            }

            let html = '<div class="d-flex flex-wrap gap-2">';

            files.forEach(file => {

                const ext = file.split('.').pop().toLowerCase();

                if (['jpg', 'jpeg', 'png', 'gif', 'webp'].includes(ext)) {
                    html += `
                        <img src="${file}" 
                             style="height:100px; border-radius:8px; cursor:pointer;"
                             onclick="viewFile('${file}')">
                    `;
                } else {
                    html += `
                        <video src="${file}" 
                               style="height:100px; border-radius:8px;" 
                               controls>
                        </video>
                    `;
                }
            });

            html += '</div>';

            $(`#${containerId}`).html(html);
        });
}

let scale = 1;
let posX = 0;
let posY = 0;
let isDragging = false;
let startX, startY;

function viewFile(filePath) {

    scale = 1;
    posX = 0;
    posY = 0;

    const ext = filePath.split('.').pop().toLowerCase();

    let content = '';

    if (['jpg', 'jpeg', 'png', 'gif', 'webp'].includes(ext)) {
        content = `
            <div id="imgContainer" style="overflow:hidden; text-align:center;">
                <img id="previewImg" src="${filePath}" 
                     style="max-width:100%; max-height:80vh; cursor:grab; display:block; margin:auto;">
            </div>

            <div style="margin-top:10px; text-align:center;">
                <button onclick="zoomIn()">🔍</button>
                <button onclick="zoomOut()">🔎</button>
                <button onclick="resetZoom()">🔄</button>
            </div>
        `;
    } else {
        content = `
            <video src="${filePath}" controls 
                   style="max-width:100%; max-height:80vh; display:block; margin:auto;"></video>
        `;
    }

    Swal.fire({
        title: 'Preview',
        html: content,
        width: 'auto',
        padding: '10px',
        showCloseButton: true,
        showConfirmButton: false,
        customClass: {
            popup: 'auto-swal'
        },
        didOpen: () => {
            const img = document.getElementById('previewImg');

            if (!img) return;

            // 🔥 Auto-fit popup to image size
            img.onload = () => {
                const maxW = window.innerWidth * 0.9;
                const maxH = window.innerHeight * 0.8;

                const ratio = Math.min(
                    maxW / img.naturalWidth,
                    maxH / img.naturalHeight,
                    1
                );

                const popup = document.querySelector('.swal2-popup');
                popup.style.width = (img.naturalWidth * ratio) + 'px';
            };

            img.addEventListener('wheel', (e) => {
                e.preventDefault();
                scale += e.deltaY * -0.001;
                scale = Math.min(Math.max(0.5, scale), 5);
                updateTransform();
            });

            img.addEventListener('mousedown', (e) => {
                isDragging = true;
                startX = e.clientX - posX;
                startY = e.clientY - posY;
                img.style.cursor = "grabbing";
            });

            window.addEventListener('mousemove', (e) => {
                if (!isDragging) return;
                posX = e.clientX - startX;
                posY = e.clientY - startY;
                updateTransform();
            });

            window.addEventListener('mouseup', () => {
                isDragging = false;
                img.style.cursor = "grab";
            });
        }
    });
}

function updateTransform() {
    const img = document.getElementById('previewImg');
    if (!img) return;

    img.style.transform = `translate(${posX}px, ${posY}px) scale(${scale})`;
}

function zoomIn() {
    scale += 0.2;
    updateTransform();
}

function zoomOut() {
    scale -= 0.2;
    scale = Math.max(0.5, scale);
    updateTransform();
}

function resetZoom() {
    scale = 1;
    posX = 0;
    posY = 0;
    updateTransform();
}


function viewAttachment(filePath) {

    const ext = filePath.split('.').pop().toLowerCase();
    let content = '';
    if (ext === 'pdf') {
        content = `
            <iframe src="${filePath}" 
                    style="width:100%; height:75vh; border:none; border-radius:8px;">
            </iframe>
        `;
    }

    else if (['jpg', 'jpeg', 'png', 'gif', 'webp'].includes(ext)) {
        content = `
            <img src="${filePath}" 
                 style="max-width:100%; max-height:75vh; border-radius:8px;">
        `;
    }
    else if (['mp4', 'mov', 'avi'].includes(ext)) {
        content = `
            <video controls style="max-width:100%; max-height:75vh; border-radius:8px;">
                <source src="${filePath}" type="video/${ext}">
            </video>
        `;
    }

    else {
        content = `
            <div style="text-align:center; padding:20px;">
                <p>Preview not available</p>
                <a href="${filePath}" target="_blank" class="btn btn-primary">
                    Download File
                </a>
            </div>
        `;
    }

    Swal.fire({
        title: 'Attachment Preview',
        html: content,
        width: '80%',
        showCloseButton: true,
        showConfirmButton: false,
        background: '#1f2937',
        color: '#fff',
        customClass: {
            popup: 'rounded-4'
        }
    });
}