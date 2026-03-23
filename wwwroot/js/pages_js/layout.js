getloginDetails();
function getloginDetails() {
    fetch('UserProfile/get_detailsForHome')
        .then(res => res.json())
        .then(data => {
            if (data.valid === 1) {

                $('#name').html(data.fullName1);
                $('#user_profile').attr('src', data.user_imgPath1 || 'images/avatar/1.png');
            }
        })
        .catch(error => console.error('Error fetching data:', error));
}