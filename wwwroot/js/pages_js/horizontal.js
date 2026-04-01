
function loadhorizontal(controlNo) {

    fetch(`/Analysis/GetHorizontal?control_no=${controlNo}`)
        .then(res => res.json())
        .then(data => {

            if (data.status === "success") {

                const hori = data.data;

                document.getElementById("s6_assembly").value = hori.s6_assembly || "";
                document.getElementById("s6_parts").value = hori.s6_parts || "";
                document.getElementById("s6_machine").value = hori.s6_machine || "";
                document.getElementById("s6_system").value = hori.s6_system || "";
                document.getElementById("s6_model").value = hori.s6_model || "";
                document.getElementById("ishorizontal").value = hori.s6_ishorizontal || "";
                document.getElementById("s6_implematation_Date").value = hori.s6_impdate || "";
                //document.getElementById("s6_implematation_Date").value = hori.s6_impdate ? hori.s6_impdate.split("T")[0] : "";

            }

        });

}



