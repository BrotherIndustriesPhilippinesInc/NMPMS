
function loadtemp(controlNo) {

    fetch(`/Analysis/GetTempAction?control_no=${controlNo}`)
        .then(res => res.json())
        .then(data => {

            if (data.status === "success") {

                const temp = data.data;

                document.getElementById("s4_assembly").value = temp.s4_assembly || "";
                document.getElementById("s4_parts").value = temp.s4_parts || "";
                document.getElementById("s4_machine").value = temp.s4_machine || "";
                document.getElementById("s4_system").value = temp.s4_system || "";
                document.getElementById("s4_detail_action_by").value = temp.s4_actionby || "";
                document.getElementById("s4_detail_attachment").value = temp.s4_detail_attachment || "";
                document.getElementById("s4_detail_action_by").value = temp.s4_pic || "";
                document.getElementById("implematation_Date").value = temp.s4_impdate || "";
                //document.getElementById("implematation_Date").value = temp.action_date ? temp.action_date.split("T")[0] : "";

                // Attachment preview
                if (temp.s4_attachment != null) {

                    const fileName = temp.s4_attachment.split(/[/\\]/).pop();

                    $('#s4_attachmentPreviewContainer').html(`
                            <div class="mt-1 text-muted small">
                                ${fileName}
                            </div>
                            <button class="btn btn-sm btn-primary mt-2"
                                    onclick="viewAttachment('${temp.s4_attachment}','3')">
                                View Attachment
                            </button>
                        `);

                } else {

                    $('#s4_attachmentPreviewContainer').html(
                        '<small class="text-muted">No Attachment</small>'
                    );

                }

            }

        });

}

              