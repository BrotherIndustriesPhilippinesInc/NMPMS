
function loadb_action(controlNo) {

    fetch(`/Analysis/GetBAction?control_no=${controlNo}`)
        .then(res => res.json())
        .then(data => {

            if (data.status === "success") {

                const b = data.data;

                document.getElementById("s7_action_judgement").value = b.s7_actionjudgement || "";
                document.getElementById("s7_action_no").value = b.s7_actionno || "";
                document.getElementById("s7_rank").value = b.s7_rank || "";
                document.getElementById("s7_pic").value = b.s7_pic || "";
            }

        });

}



