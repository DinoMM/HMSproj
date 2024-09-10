
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
            // Check if the click occurred outside the popover
            if (!$(e.target).closest('.popover').length && !$(e.target).is(element)) {
                $(element).popover('hide');
                // Remove the event listener after hiding the popover
                $(document).off('click.popover');
            }
        });
    }
}

