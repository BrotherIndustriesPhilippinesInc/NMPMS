
function loadAnalysis(controlNo) {

    fetch(`/Analysis/GetAnalysis?control_no=${controlNo}`)
        .then(res => res.json())
        .then(data => {

            if (data.status === "success") {
                const steps = '2'

                const d = data.data;

                document.getElementById("analysis_cause").value = d.analysis_cause || "";
                document.getElementById("defect_details").value = d.defect_details || "";
                document.getElementById("problem_category").value = d.problem_category || "";
                document.getElementById("analysis_by").value = d.analysis_by || "";
                //document.getElementById("finish_date").value = d.finish_date ? d.finish_date.split("T")[0] : "";
                document.getElementById("finish_date").value = d.finish_date || "";
                document.getElementById("problem_rank").value = d.problem_rank || "";

                // attachment preview
                if (d.attachment != null) {
                    //document.getElementById("attachmentPreviewContainer").innerHTML =
                    //    `<a href="/${d.attachment}" target="_blank">View Attachment</a>`;

                    //viewAttachment(filename, steps)
                    const fileName = d.attachment.split(/[/\\]/).pop();
                    console.log(fileName);

                    $('#attachmentPreviewContainer1').html(`
                        <div class="mt-1 text-muted small">
                            ${fileName}
                        </div>
                        <button class="btn btn-sm btn-primary mt-2" 
                                onclick="viewAttachment('${fileName}','${steps}')">
                            View Attachment
                        </button>
                    `);
                } else {
                    $('#attachmentPreviewContainer').html('<small class="text-muted">No Attachment</small>');
                }

                // image preview
                //if (d.image_cause) {
                //    document.getElementById("photoPreviewContainer").innerHTML =
                //        `<img src="/${d.image_cause}" style="max-width:150px;">`;
                //}
                if (d.image_cause) {

                    const cleanPath = d.image_cause.replace(/\\/g, "/");

                    const ext = cleanPath.split('.').pop().toLowerCase();

                    let html = '';

                    if (['jpg', 'jpeg', 'png', 'gif', 'webp'].includes(ext)) {
                        html = `
                            <img src="${cleanPath}" 
                                 style="height:120px; border-radius:8px; cursor:pointer;"
                                 onclick="viewFile('${cleanPath}')">
                        `;
                                    } else {
                                        html = `
                            <video src="${cleanPath}" 
                                   style="height:120px;" controls>
                            </video>
                        `;
                    }

                    $('#photoPreviewContainer1').html(html);

                } else {
                    $('#photoPreviewContainer1').html('<small class="text-muted">No Photo Uploaded</small>');
                }

            }

        });

}