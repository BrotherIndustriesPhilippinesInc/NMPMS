
function loadper(controlNo) {

    fetch(`/Analysis/GetPerAction?control_no=${controlNo}`)
        .then(res => res.json())
        .then(data => {

            if (data.status === "success") {

                const per = data.data;

                document.getElementById("s5_assembly").value = per.s5_assembly || "";
                document.getElementById("s5_parts").value = per.s5_parts || "";
                document.getElementById("s5_machine").value = per.s5_machine || "";
                document.getElementById("s5_system").value = per.s5_system || "";
                document.getElementById("s5_pic").value = per.s5_pic || "";
                document.getElementById("s5_implematation_Date").value = per.s5_impdate || "";
                //document.getElementById("s5_implematation_Date").value = per.s5_impdate ? per.s5_impdate.split("T")[0] : "";
                if (per.s5_attachment != null) {

                    const fileName = per.s5_attachment.split(/[/\\]/).pop();

                    $('#s5_attachmentPreviewContainer').html(`
                            <div class="mt-1 text-muted small">
                                ${fileName}
                            </div>
                            <button class="btn btn-sm btn-primary mt-2"
                                    onclick="viewAttachment('${per.s5_attachment}','5')">
                                View Attachment
                            </button>
                        `);

                } else {

                    $('#s5_attachmentPreviewContainer').html(
                        '<small class="text-muted">No Attachment</small>'
                    );

                }
            }

        });

}



