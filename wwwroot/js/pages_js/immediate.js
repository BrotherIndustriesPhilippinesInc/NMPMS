
function loadImmediateAction(controlNo) {

    fetch(`/Analysis/GetImmediateAction?control_no=${controlNo}`)
        .then(res => res.json())
        .then(data => {

            if (data.status === "success") {

                const d = data.data;

                document.getElementById("assembly").value = d.assembly || "";
                document.getElementById("parts").value = d.parts || "";
                document.getElementById("machine").value = d.machine || "";
                document.getElementById("system").value = d.system || "";

                document.getElementById("fg_treatment").value = d.fg_treatment || "";
                document.getElementById("process_change").value = d.process_change || "";
                document.getElementById("wi_change").value = d.wi_change || "";
                document.getElementById("re_education").value = d.re_education || "";
                document.getElementById("change_manpower").value = d.change_manpower || "";
                document.getElementById("other_action").value = d.other || "";

                document.getElementById("action_by").value = d.action_by || "";
                document.getElementById("action_date").value = d.action_date ? d.action_date.split("T")[0] : "";

                // Attachment preview
                if (d.attachment != null) {

                    const fileName = d.attachment.split(/[/\\]/).pop();

                    $('#attachmentPreviewContainerIA').html(`
                            <div class="mt-1 text-muted small">
                                ${fileName}
                            </div>
                            <button class="btn btn-sm btn-primary mt-2"
                                    onclick="viewAttachment('${fileName}','3')">
                                View Attachment
                            </button>
                        `);

                } else {

                    $('#attachmentPreviewContainerIA').html(
                        '<small class="text-muted">No Attachment</small>'
                    );

                }

            }

        });

}