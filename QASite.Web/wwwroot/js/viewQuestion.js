$(() => {
    $("#like-question").on('click', () => {
        const questionId = $("#like-question").data('question-id');
        $.post('/Home/AddQuestionLike' { questionId }, () => {
            updateLikes();
            $("#like-question").addClass('text-danger');
            $("#like-question").unbind('click');
        });
    });

    const updateLikes = () => {
        const questionId = $("#likes-count").data('questionId');
        $.get('/Home/getLikes', { questionId }, result => {
            $("#likes-count").text(result.likes);
        });
    };
   setInterval(updateLikes, 1000);
});