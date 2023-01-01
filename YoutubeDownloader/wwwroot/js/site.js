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
            $('#video-div').append('<iframe id="video-frame" style="margin-top: 10px;max-width: 445px;" width="100%" height="250" src="' + $("#input-video-url").val().replaceAll('.com/watch?v=', '.com/embed/') + '" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"></iframe>');
            $('#result-table').hide();
            $('#result-table > tbody').empty();
            window.scrollTo(0, document.body.scrollHeight);
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
    $('#result-table > tbody').empty();
    $('#dl-container').hide();
    $('#result-table').hide();
    var url = $("#input-video-url").val();
    $.ajax({
        url: './Home/Parse?videourl=' + $("#input-video-url").val(),
        type: 'POST',
        success: function (data) {
            try {
                let res = JSON.parse(data);
                $('#counter').text(numberWithCommas(res.CountRequests))
                res.FormatInfos.forEach(function (item, i) {
                    $('#result-table > tbody').append('<tr><th scope="row"><i class="' + item.Icon + '"></i></th><td>' + item.Extension + '</td><td>' + item.Resolution + '</td><td>' + item.Bitrate + '</td><td>' + item.Size + '</td><td><button id="dl-' + item.ID + '" data-url="' + url + '" data-id="' + item.ID + '" data-meme="' + item.MEME + '" class="btn btn-primary" onclick="download(event,\'dl-' + item.ID + '\')" style="width: 100%;"><i class="bi bi-cloud-download" style="margin-right:5px"></i><span>Convert</span><img style="display:none; margin-left:5px" src="./images/loading.gif" width="20" height="20" /></button></td></tr>')
                });
                $('#result-table').show();
                window.scrollTo(0, document.body.scrollHeight);
            } catch (err) {
                console.error(err)
                console.error(data)
                alert("Failed to parse video, please try again later!");
            }
            done();
        },
        error: function (request, error) {
            console.error(request)
            console.error(error)
            alert("Failed to parse video, please try again later!");
            done();
        }
    });
}

function numberWithCommas(x) {
    return x.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
}

function download(e, btnId) {
    let btn = $('#' + btnId);
    btn.find('img').first().show();
    btn.attr('disabled', 'disabled');
    $('#dl-container').hide();

    $.ajax({
        url: './Home/Download?videourl=' + btn.attr('data-url') + '&formatId=' + btn.attr('data-id'),
        type: 'POST',
        success: function (data) {
            try {
                btn.find('img').first().hide();
                btn.removeAttr('disabled');
                downloadURI(data, 'videoplayback', btn.attr('data-meme'));
            } catch (err) {
                console.error(err)
                console.error(data)
                alert("Failed to get download link, please try again later!");
            }
        },
        error: function (request, error) {
            console.error(request)
            console.error(error)
            alert("Failed to get download link, please try again later!");
            btn.find('img').first().hide();
            btn.removeAttr('disabled');
        }
    });
}

function downloadURI(uri, name, meme) {
    $("#dl-div").empty();
    if (meme.toLowerCase().startsWith('video')) {
        $('#dl-div').append('<video id="dl-frame" style="height: 250px;width: 100%;border-radius: 5px;" src="' + uri + '" controls></video>');
        $('#dl-type').text('video');
        $('#dl-type-cap').text('Video');
    } else {
        $('#dl-div').append('<audio id="dl-frame" style="height: 25px;width: 100%;border-radius: 5px;" src="' + uri + '" controls></audio>');
        $('#dl-type').text('audio');
        $('#dl-type-cap').text('Audio');
    }
    $('#dl-container').show();
    window.scrollTo(0, document.body.scrollHeight);
}

function done() {
    $("#action-img").hide();
    $("#action-label").text('Parse URL');
    $('#btn-process').removeAttr('disabled');
    $("#input-video-url").removeAttr('disabled');
}