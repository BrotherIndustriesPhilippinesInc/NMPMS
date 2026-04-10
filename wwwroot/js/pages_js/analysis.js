
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

                if (d.attachment != null) {
                    const fileName = d.attachment.split(/[/\\]/).pop();
                    console.log(fileName);

                    $('#attachmentPreviewContainer1').html(`
                        <div class="mt-1 text-muted small">
                            ${fileName}
                        </div>
                        <button class="btn btn-sm btn-primary mt-2" 
                                onclick="viewAttachment('${d.attachment}')">
                            View Attachment
                        </button>
                    `);
                } else {
                    $('#attachmentPreviewContainer').html('<small class="text-muted">No Attachment</small>');
                }
                loadProblemFiles(controlNo, 2, "photoPreviewContainer1");
               

            }

        });

}