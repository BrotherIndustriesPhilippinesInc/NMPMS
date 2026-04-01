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

                if (pheno.attachment_name) {
                    $('#attachmentPreviewContainer').html(`
                        <div class="mt-1 text-muted small">
                            ${pheno.attachment_name}
                        </div>
                        <button class="btn btn-sm btn-primary mt-2" 
                                onclick="viewAttachment('${pheno.attachment_name}','1')">
                            View Attachment
                        </button>
                    `);
                } else {
                    $('#attachmentPreviewContainer').html('<small class="text-muted">No Attachment</small>');
                }

                fetch(`/Analysis/GetProblemFiles?control_no=${controlNo}`)
                    .then(res => res.json())
                    .then(files => {

                        if (!files.length) {
                            $('#photoPreviewContainer').html('<small class="text-muted">No Photo Uploaded</small>');
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

                        $('#photoPreviewContainer').html(html);
                    });
            }
        });
}


function viewFile(filePath) {

    const ext = filePath.split('.').pop().toLowerCase();

    let content = '';

    console.log(filePath);

    if (['jpg', 'jpeg', 'png', 'gif', 'webp'].includes(ext)) {
        content = `<img src="${filePath}" style="max-width:100%; max-height:80vh;">`;
    } else {
        content = `<video src="${filePath}" controls style="max-width:100%; max-height:80vh;"></video>`;
    }

    Swal.fire({
        title: 'Preview',
        html: content,
        width: '90%',
        showCloseButton: true,
        showConfirmButton: false
    });
}