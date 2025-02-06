
//Ak dany element s id obsahuje class tak ju odstrani inak prida
function switchStyleClass(Id, classname) {
    var element = document.getElementById(Id);
    if (element != null) {
        if (element.classList.contains(classname)) {
            element.classList.remove(classname);
        } else {
            element.classList.add(classname);
        }
    }
}

//prida na podobu jedneho kliku class 
function addStyleClassUntilClick(Id, classname) {
    var element = document.getElementById(Id);
    if (element != null) {
        element.classList.add(classname);

        // Attach a click event listener to the document
        $(document).on('click.styleClass', function (e) {
            // Check if the click occurred outside the element
            if (!$(e.target).closest('#' + Id).length && !$(e.target).is(element)) {
                element.classList.remove(classname);
                // Remove the event listener after removing the class
                $(document).off('click.styleClass');
            }
        });
    }
}

function addWarningPopOverUntilClick(id, content, direction)
{
    addStyleClassUntilClick(id, "border-warning");
    showPopover(id, content, direction);
}

function addInfoPopOverUntilClick(id, content, direction) {
    addStyleClassUntilClick(id, "border-info");
    showPopover(id, content, direction);
}

//zobrazi popover pri elemente s danym id, + text a smer  ("bottom", "right", "left", "top")
function showPopover(id, content, direction) {
    var element = document.getElementById(id);
    if (element) {
        $(element).popover({
            content: content,
            trigger: 'manual',
            placement: direction // Position the popover on the left
        });
        $(element).popover('show');

        // Attach a click event listener to the document
        $(document).on('click.popover', function (e) {
            // Check if the click occurred outside the popover or on the input element itself
            if (!$(e.target).closest('.popover').length || $(e.target).is(element)) {
                $(element).popover('hide');
                // Remove the event listener after hiding the popover
                $(document).off('click.popover');
            }
        });
    }
}

//Pomoc od AI
window.sortTableByTh = function () {
    const thElms = document.querySelectorAll(".th-sortable");

    thElms.forEach(thElm => {
        thElm.addEventListener("dblclick", function () {
            const table = thElm.closest("table");
            const columnIndex = Array.from(thElm.parentNode.children).indexOf(thElm);
            const rows = Array.from(table.rows).slice(1); // Exclude header row

            // Sort rows
            const sortedRows = rows.slice().sort((a, b) => {
                const cellA = a.cells[columnIndex].innerText.toLowerCase();
                const cellB = b.cells[columnIndex].innerText.toLowerCase();
                return cellA.localeCompare(cellB);
            });

            // Check if the sorted list is the same as the current list
            const isAlreadySorted = rows.every((row, index) => row === sortedRows[index]);

            // If already sorted, reverse the order
            if (isAlreadySorted) {
                sortedRows.reverse();
            }

            // Append sorted rows back to the table
            sortedRows.forEach(row => table.tBodies[0].appendChild(row));
        });
    });
}

//vyčistí input field s daným id, bez kontroly na existenciu
function clearInputField(id) {
    document.getElementById(id).value = '';
}

function setInputField(id, newText) {
    document.getElementById(id).value = newText;
}

function getInputField(inputId) {
    return document.getElementById(inputId).value;
}

//focusne input field s daným id
function focusInputField(id) {
    const element = document.getElementById(id);
    if (element) {
        element.focus();
    }
}


//inicializuje vsetky tooltipy na stranke, treba spustit na OnAfterRenderAsync po prvy krat. Pre AUTOMATICKE zobrazenie tooltipov AI
window.initializeTooltipsAuto = () => {
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.forEach(function (tooltipTriggerEl) {
        new bootstrap.Tooltip(tooltipTriggerEl);
    });
};

//inicializuje tooltip s danym id, treba spustit na OnAfterRenderAsync po prvy krat
window.initializeTooltip = (id) => {
    var tooltipTriggerEl = document.getElementById(id);
    if (tooltipTriggerEl) {
        new bootstrap.Tooltip(tooltipTriggerEl);
    }
};

// Aktualituje tooltip s danym id na novy text (treba inicializovat js funciu initializeTooltipsAuto)
window.updateTooltipOnHover = (id, newText) => {
    var tooltipTriggerEl = document.getElementById(id);
    if (tooltipTriggerEl) {
        // Retrieve the existing tooltip instance
        var tooltip = bootstrap.Tooltip.getInstance(tooltipTriggerEl);
        if (tooltip) {
            //  Update the tooltip's content dynamically
            tooltip.setContent({ '.tooltip-inner': newText });
        }
    }
};

//zmeni zobrazenie tooltipu s danym id na show alebo hide
window.updateTooltipShow = (id, showv) => {
    var tooltipTriggerEl = document.getElementById(id);
    if (tooltipTriggerEl) {
        var tooltip = bootstrap.Tooltip.getInstance(tooltipTriggerEl);
        if (tooltip) {
            if (showv) {
                tooltip.show();
            } else {
                tooltip.hide();
            }
        }
    }
};

//pre checkboxy, ktore mozu mat null tak spusti tuto metodu ak su null
window.setIndeterminate = (id) => {
    const checkbox = document.getElementById(id);
    if (checkbox) {
        checkbox.indeterminate = true;
    }
};

function getInputValueById(elementId) {
    const inputElement = document.getElementById(elementId);
    return inputElement ? inputElement.value : null;
}


//zachytenie hodnoty scrollbaru
window.getScrollPosition = (elementId) => {
    var element = document.getElementById(elementId);
    return element ? element.scrollTop : 0;
};

//nastavenie hodnoty scrollbaru
window.setScrollPosition = (elementId, position) => {
    var element = document.getElementById(elementId);
    if (element) {
        element.scrollTop = position;
    }
};



