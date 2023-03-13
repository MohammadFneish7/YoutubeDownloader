String.prototype.replaceAll = function (strReplace, strWith) {
    // See http://stackoverflow.com/a/3561711/556609
    var esc = strReplace.replace(/[-\/\\^$*+?.()|[\]{}]/g, '\\$&');
    var reg = new RegExp(esc, 'ig');
    return this.replace(reg, strWith);
};

$(document).ready(function () {
    $("#input-video-url").on('input', function () {
        loadiframe();
    });

    loadiframe();
});

function loadiframe() {
    $('#video-div').empty();
    let url = $("#input-video-url").val();
    if (url) {
        if (url.toLowerCase().includes("youtu.be") || url.toLowerCase().includes("youtube.com")) {
            $('#btn-process').removeAttr('disabled');
            $('#video-div').append('<iframe id="video-frame" style="margin-top: 10px;max-width: 610px;" width="100%" height="300" src="' + $("#input-video-url").val().replaceAll('.com/watch?v=', '.com/embed/') + '" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"></iframe>');
            $('#result-container').hide();
            $('#result-container > table > tbody').empty();
            //window.scrollTo(0, document.body.scrollHeight);
        }
    } else {
        $('#btn-process').attr('disabled', 'disabled');
    }
}

function parse() {
    $("#action-img").show();
    $('#btn-process').attr('disabled', 'disabled');
    $("#input-video-url").attr('disabled', 'disabled');
    $("#action-label").text('Processing...');
    $('#result-container > table > tbody').empty();
    $('#dl-container').hide();
    $('#result-container').hide();
    var url = $("#input-video-url").val();
    $.ajax({
        url: './Home/Parse?videourl=' + $("#input-video-url").val(),
        type: 'POST',
        success: function (data) {
            try {
                let res = JSON.parse(data);
                $('#counter').text(numberWithCommas(res.CountRequests))
                res.FormatInfos.forEach(function (item, i) {
                    $('#result-container > table > tbody').append('<tr><th scope="row"><i class="' + item.Icon + '"></i></th><td>' + item.MEME + '</td><td>' + item.Extension + '</td><td>' + item.Resolution + '</td><td>' + item.Bitrate + '</td><td>' + item.Size + '</td><td><a id="dl-' + item.ID + '" role="button" href="' + item.Url + '" class="btn btn-danger" target="_blank" style="width: 100%;"><i class="bi bi-cloud-download" style="margin-right:5px"></i><span class="d-none d-md-inline">Download</span></a></td></tr>')
                });
                $('#result-container').show();

                var scrollDiv = document.getElementById("result-container").offsetTop;
                window.scrollTo({ top: scrollDiv-90, behavior: 'smooth' });

                //document.getElementById("result-container").scrollIntoView();
                //window.scrollTo(0, document.body.scrollHeight);
            } catch (err) {
                console.error(err)
                console.error(data)
                alert("Failed to parse video, please try again later!");
                location.reload();
                return false;
            }
            done();
        },
        error: function (request, error) {
            console.error(request)
            console.error(error)
            alert("Failed to parse video, please try again later!");
            location.reload();
            return false;
        }
    });
}

function numberWithCommas(x) {
    return x.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
}

function done() {
    $("#action-img").hide();
    $("#action-label").text('Parse URL');
    $('#btn-process').removeAttr('disabled');
    $("#input-video-url").removeAttr('disabled');
}