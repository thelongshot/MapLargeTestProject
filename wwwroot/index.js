/* global Tree */
'use strict';
$(function () {
    // Need the native DOM objects.
    const uploadModal = document.getElementById("upload");
    const searchModal = document.getElementById("search");

    let currPath = '';
    let folderId = '';

    // Hide the processing div at startup.
    $('#processing').hide();

    // Create the treeview
    $('#tree').tree();

    $('#tree').on('tree.click', function (e) {
        // Normally, the id and path would be different
        // but given there is no backend storage, id is also the path. 
        updateFileList(e.node.id, e.node.path);
    });

    // Upload Dialog events
    $('#upload-file-dialog').on('click', function () {
        if (currPath === '') {
            alert('Select a folder to upload a file');
        } else {
            $('#upload-file').val('');
            uploadModal.showModal();
        }
    })

    $('#cancel-upload-modal').on('click', function () {
        uploadModal.close();
    })

    $('#submit-upload-modal').on('click', function () {
        uploadModal.close();

        // $('#upload-file')
        const formData = new FormData();
        let file = $('#upload-file').prop("files")[0];
        if (!file) {
            alert('A file needs to be selected');
        } else {
            formData.append('UploadPath', currPath);
            formData.append('File', file);
            $.ajax({
                url: '/Files/UploadFile',
                type: 'POST',
                data: formData,
                cache: false,
                contentType: false,
                processData: false,
                success: (result) => {
                    updateFileList(folderId, currPath)
                    alert('Successfully uploaded');
                }
            });
        }
    })

    // Search dialog events
    $('#search-file-dialog').on('click', function () {
        $('#search-files').val('');
        searchModal.showModal();
    })


    $('#cancel-search-modal').on('click', function () {
        searchModal.close();
    })

    $('#search-search-modal').on('click', function () {
        searchModal.close();

        let search = $('#search-files').val();
        if (search === '') {
            alert('A search term must be provided.');
        } else {
            processing(true);
            $.ajax({
                url: '/Files/FindFiles?search=' + encodeURIComponent(search),
                type: 'GET',
                cache: false,
                contentType: false,
                processData: false,
                success: (result) => {
                    processing(false);
                    clear();
                    const fileListContainer = $('#file-list');
                    fileListContainer.empty();
                    if (result.length > 0) {
                        populateFileList(result);
                        $('#folder-path').text('Search results for: ' + search);
                        $('#file-count').text('results: ' + result.length + ' files');
                    } else {
                        fileListContainer.append('No Files Found');
                    }
                },
                error: () => {
                    processing(false);
                }
            });
        }
    })

    function updateFileList(id, path) {
        processing(true);
        $.ajax({
            url: 'Files/GetFiles?path=' + encodeURIComponent(id),
            success: function (result) {
                processing(false);

                folderId = id;
                currPath = path;
                $('#folder-path').text('Selected: \\' + path);
                $('#upload-path').text('\\' + path);
                $('#file-list').empty();

                if (result.length > 0) {
                    const totalBytes = populateFileList(result);

                    $('#folder-size').text(totalBytes + ' bytes');
                    $('#file-count').text(result.length + ' files');
                } else {
                    $('#file-list').append('No Files');
                    $('#folder-size').text('');
                    $('#file-count').text('');
                }
            },
            error: () => {
                processing(false);
            }
        })
    };

    function populateFileList(result) {
        let totalBytes = 0
        result.forEach(function (file) {
            $('#file-list')
                .append($('<div>')
                    .append($('<a>', {
                        text: file.name,
                        href: '/Files/DownloadFile?path=' + encodeURIComponent(file.path)
                    }))
                )
                .append($('<div>').text(file.size + ' bytes'))
            totalBytes += file.size;
        });

        return totalBytes;
    }

    function clear() {
        // Clear the node selection, the file list and all data fields.
        $('#tree').tree('selectNode', null);
        $('#folder-path').text('');
        $('#folder-size').text('');
        $('#file-count').text('');
        $('#file-list').empty();
        folderId = '';
        currPath = '';
    }

    function processing(busy) {
        if (busy) {
            $('#processing').show();
            $('#file-list').hide();
        } else {
            $('#processing').hide();
            $('#file-list').show();
        }
    }
});
