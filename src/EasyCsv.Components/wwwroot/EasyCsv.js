async function downloadFileFromStreamEasyCsv(fileName, contentStreamReference) {
    const arrayBuffer = await contentStreamReference.arrayBuffer();
    const blob = new Blob([arrayBuffer]);

    const url = URL.createObjectURL(blob);

    triggerFileDownload(fileName, url);

    URL.revokeObjectURL(url);
}

function triggerFileDownload(fileName, url) {
    const anchorElement = document.createElement('a');
    anchorElement.href = url;

    if (fileName) {
        anchorElement.download = fileName;
    }

    anchorElement.click();
    anchorElement.remove();
}

// Function to set tabindex="-1" on all .mud-list-item elements inside .csv-processing-popover
function setTabIndex(parent) {
    const items = parent.querySelectorAll('.mud-list-item');
    items.forEach(item => {
        item.setAttribute('tabindex', '-1');
    });
}

// Function to observe specific popover for .mud-list-item additions or changes
function observePopover(popover) {
    const popoverObserver = new MutationObserver(() => {
        setTabIndex(popover);
    });

    popoverObserver.observe(popover, {
        childList: true,
        subtree: true,
        attributes: false
    });
}

// Observer for detecting the creation of any .csv-processing-popover elements in the DOM
const observer = new MutationObserver((mutations) => {
    mutations.forEach(mutation => {
        mutation.addedNodes.forEach(node => {
            if (node.nodeType === Node.ELEMENT_NODE && node.classList.contains('csv-processing-popover')) {
                // Set tabindex for any existing .mud-list-item elements immediately
                setTabIndex(node);
                // Start observing this specific popover for changes
                observePopover(node);
            }
        });
    });
});

// Start observing the document body for the creation of .csv-processing-popover elements
observer.observe(document.body, {
    childList: true,
    subtree: true
});