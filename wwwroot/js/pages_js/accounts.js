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

$('#biph_id').on('keydown', function (e) {
    if (!e) return; // safety check
    if (e.key === "Enter") { // more reliable than e.which
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

$(document).on("click", "#save_user", function (e) {
    e.preventDefault();

    const form = document.getElementById('form_submit');
    const formData = new FormData(form);

    //formData.append('fullname', $('#fullname').val());
    //formData.append('adid', $('#adid').val());
    //formData.append('section', $('#section').val());
    //formData.append('biph_id', $('#biph_id').val());
    //formData.append('position', $('#position').val());
    //formData.append('email', $('#email').val());

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

                //save_to_iportal(fullname, email, section, position, adid, biph_id);
                save_to_iportal(
                    $('#fullname').val(),
                    $('#email').val(),
                    $('#section').val(),
                    $('#position').val(),
                    $('#adid').val(),
                    $('#biph_id').val()
                );
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


function save_to_iportal(Name, email, Section, Position, ADID, Empno) {
    var approver_number = 0;
    var system_id = 84;

     if(Position === "Supervisor" || Position === "Junior Supervisor" || Position === "Senior Supervisor" || Position === "Senior Staff" || Position === "Senior Engineer"){
       approver_number = 1;
     }else if(Position ==="Manager" || Position === "Senior Manager"){
       approver_number = 2
     }else{
       approver_number = 0
     }

    $.ajax({
        "url": 'http://apbiphbpsts01:8080/CASAPI/api/register',
        "type": 'POST',
        "data": {
            system_id: system_id,
            system_name: "New Model Problem Management System",
            approver_number: approver_number,
            full_name: Name,
            email: email,
            section: Section,
            position: Position,
            adid: ADID,
            employee_number: Empno
        },
        "success": function (response) {
            console.log(response);
        },
        "error": function (xhr, status, error) {
            console.error('AJAX error:', status, error);
        }
    });
}