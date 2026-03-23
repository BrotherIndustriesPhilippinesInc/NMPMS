getuserDetails();
function getuserDetails() {
    fetch('UserProfile/get_details')
        .then(res => res.json())
        .then(data => {
            if (data.valid === 1) {
                let typeLabel = ""
                switch (parseInt(data.userlevel)) {
                    case 1: typeLabel = "ADMIN"; break;
                    case 2: typeLabel = "MGR"; break;
                    case 3: typeLabel = "SPV"; break;
                    case 4: typeLabel = "STAFF/ENGINEER"; break;
                    default: typeLabel = "UNKNOWN"; break;
                }
                $('#fullname').val(data.fullName);
                $('#name1').html(data.fullName);
                $('#adid').html(data.adid);
                $('#email').val(data.email);
                $('#position').val(data.position);
                $('#section').val(data.section);
                $('#section1').html(data.section);
                $('#userlevel').html(typeLabel);
                $('#date_added').val(data.formattedDate);
                $('#profileImage').attr('src', data.user_imgPath || 'images/avatar/1.png');
                $('#user_profile').attr('src', data.user_imgPath || 'images/avatar/1.png');
            }
        })
        .catch(error => console.error('Error fetching data:', error));
}


document.addEventListener("DOMContentLoaded", function () {

    const profileImage = document.getElementById("profileImage");
    const uploadTrigger = document.getElementById("uploadTrigger");
    const imageUpload = document.getElementById("imageUpload");

    uploadTrigger.addEventListener("click", () => imageUpload.click());
    profileImage.addEventListener("click", () => imageUpload.click());

    imageUpload.addEventListener("change", function () {
        const file = this.files[0];

        if (!file) return;

        if (!file.type.startsWith("image/")) {
            alert("Only image files are allowed");
            return;
        }

        if (file.size > 2 * 1024 * 1024) {
            alert("Max file size is 2MB");
            return;
        }
        const reader = new FileReader();
        reader.onload = e => profileImage.src = e.target.result;
        reader.readAsDataURL(file);
        uploadImage(file);
    });

    function uploadImage(file) {
        const formData = new FormData();
        formData.append("file", file);

        fetch("/UserProfile/upload_image", {
            method: "POST",
            body: formData
        })
            .then(res => res.json())
            .then(data => {
                if (data.success) {
                    console.log("Uploaded:", data.path);
                } else {
                    alert(data.message);
                }
            })
            .catch(err => console.error(err));
    }

});