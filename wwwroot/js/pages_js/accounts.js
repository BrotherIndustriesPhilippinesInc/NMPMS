fetch('Accounts/fetch_accounts')
    .then(response => response.json()) // Convert response to JSON
    .then(data => {
        console.log("Received Data:", data); // Debugging log
        users_dataTable(data);
        // console.log(Array.isArray(data), data);
    })
    .catch(error => console.error('Error fetching data:', error));
function users_dataTable(data) {
    let table = $('#usertbl').DataTable();
    table.destroy();
    let tableBody = document.getElementById("users_tbody");

    tableBody.innerHTML = "";


    data.forEach((item, index) => {
       let typeLabel = "";
       let status = "";
       switch (parseInt(item.type)) {
            case 1: typeLabel = "ADMIN"; break;
            case 2: typeLabel = "MGR"; break;
            case 3: typeLabel = "SPV"; break;
            case 4: typeLabel = "STAFF/ENGINEER"; break;
            default: typeLabel = "UNKNOWN"; break;
        }
        switch (parseInt(item.status)) {
            case 1: status = "ACTIVE"; break;
            case 2: status = "INACTIVE"; break;
            default: status = "UNKNOWN"; break;
        }
        let row = `<tr>
                  <td>${index + 1}</td>
                  <td>
                    <div class="avatar-wrapper">
                      <img src="${item.user_imgPath}" class="avatar-img" />
                    </div>
                  </td>
                  <td>${item.adid}</td>
                  <td>${item.name}</td>
                  <td>${item.section}</td>
                  <td>${typeLabel}</td>
                  <td>${status}</td>
                  <td>
                    <button class="btn btn-info btn-sm">Update</button>
                  </td>
                </tr>`;
        tableBody.innerHTML += row;
    });
    $('#usertbl').DataTable({
        destroy: true, 
        responsive: true,
        autoWidth: false
    });
}

$(document).ready(function () {
    $('#biph_id').on('keypress', function (e) {
        if (e.which == 13) {
            let empno = $(this).val();

            $.post('Accounts/get_details', { empNo: empno })
                .done(res => {
                    if (res.valid === 1) {
                        $('#fullname').val(res.fullName);
                        $('#adid').val(res.adid);
                        $('#email').val(res.email);
                        $('#position').val(res.position);
                        $('#section').val(res.section);
             }
          });
        }
    });
});

$(document).on("click", "#save_user", function (e) {
    e.preventDefault();

    const form = document.getElementById('form_submit');
    const formData = new FormData(form);

    fetch('Accounts/save_user', {
        method: "POST",
        body: formData
    })
        .then(res => res.json())
        .then(data => {
            if (data.status === "success") {
                Swal.fire({
                    title: "Success",
                    text: "User saved successfully!",
                    icon: "success"
                });
            } else {
                Swal.fire({
                    title: "Error",
                    text: data.message || "Something went wrong",
                    icon: "error"
                });
            }
        })
        .catch(err => {
            console.error(err);
            Swal.fire({
                title: "Error",
                text: "Request failed",
                icon: "error"
            });
        });
});
