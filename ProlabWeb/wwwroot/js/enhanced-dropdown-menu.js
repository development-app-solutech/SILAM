/**
 * COMPLETE DROPDOWN REPLACEMENT - Bypasses KT UI entirely
 * Creates new dropdown elements from scratch
 */

console.log('🔥 COMPLETE DROPDOWN REPLACEMENT LOADED!');

(function() {
    'use strict';

    let currentOpenDropdown = null;
    let initialized = false;
    let observer = null;

    function createCustomDropdown(originalDropdown) {
        const menuItems = originalDropdown.querySelectorAll('.kt-menu-item');
        const dropdownHtml = Array.from(menuItems).map(item => {
            const link = item.querySelector('a');
            if (!link) return '';
            
            const icon = link.querySelector('i');
            const title = link.querySelector('.kt-menu-title');
            
            // Get icon class or assign default based on action type - ALWAYS ASSIGN AN ICON
            let iconClass = 'ki-filled ki-dots-horizontal'; // Default fallback
            
            // Get text content for analysis
            const titleText = title ? title.textContent.trim().toLowerCase() : '';
            console.log('🔍 Processing menu item:', titleText);
            
            // First check if there's an existing icon
            if (icon && icon.className && icon.className.trim()) {
                iconClass = icon.className.trim();
                console.log('✅ Using existing icon:', iconClass);
            } else {
                // Assign specific icons based on text content - MORE COMPREHENSIVE
                console.log('🎯 Assigning icon based on text:', titleText);
                
                if (titleText.includes('modifier') || titleText.includes('edit') || titleText.includes('éditer')) {
                    iconClass = 'ki-filled ki-pencil';
                    console.log('📝 Assigned pencil icon for edit action');
                } else if (titleText.includes('imprimer') || titleText.includes('print') || titleText.includes('impression')) {
                    iconClass = 'ki-filled ki-printer';
                    console.log('🖨️ Assigned printer icon for print action');
                } else if (titleText.includes('saisir') || titleText.includes('résultats') || titleText.includes('results') || titleText.includes('ajouter') || titleText.includes('add')) {
                    iconClass = 'ki-filled ki-add-files';
                    console.log('📄 Assigned add-files icon for input action');
                } else if (titleText.includes('supprimer') || titleText.includes('delete') || titleText.includes('effacer')) {
                    iconClass = 'ki-filled ki-trash';
                    console.log('🗑️ Assigned trash icon for delete action');
                } else if (titleText.includes('voir') || titleText.includes('view') || titleText.includes('détail') || titleText.includes('details') || titleText.includes('afficher')) {
                    iconClass = 'ki-filled ki-eye';
                    console.log('👁️ Assigned eye icon for view action');
                } else if (titleText.includes('télécharger') || titleText.includes('download') || titleText.includes('export')) {
                    iconClass = 'ki-filled ki-down';
                    console.log('⬇️ Assigned download icon for download action');
                } else if (titleText.includes('copier') || titleText.includes('copy') || titleText.includes('dupliquer')) {
                    iconClass = 'ki-filled ki-copy';
                    console.log('📋 Assigned copy icon for copy action');
                } else if (titleText.includes('partager') || titleText.includes('share') || titleText.includes('envoyer')) {
                    iconClass = 'ki-filled ki-share';
                    console.log('📤 Assigned share icon for share action');
                } else if (titleText.includes('valider') || titleText.includes('validate') || titleText.includes('approuver')) {
                    iconClass = 'ki-filled ki-check';
                    console.log('✅ Assigned check icon for validate action');
                } else if (titleText.includes('annuler') || titleText.includes('cancel') || titleText.includes('rejeter')) {
                    iconClass = 'ki-filled ki-cross';
                    console.log('❌ Assigned cross icon for cancel action');
                } else {
                    // Use generic action icon as fallback
                    iconClass = 'ki-filled ki-setting-3';
                    console.log('⚙️ Assigned generic action icon as fallback for:', titleText);
                }
            }
            
            const finalTitleText = title ? title.textContent.trim() : 'Action';
            const href = link.getAttribute('href') || '#';
            
            return `
                <div class="custom-dropdown-item">
                    <a href="${href}" class="custom-dropdown-link">
                        <i class="${iconClass}"></i>
                        <span>${finalTitleText}</span>
                    </a>
                </div>
            `;
        }).join('');
        
        const customDropdown = document.createElement('div');
        customDropdown.className = 'custom-dropdown-menu';
        customDropdown.innerHTML = dropdownHtml;
        customDropdown.style.cssText = `
            position: fixed;
            z-index: 999999;
            background: linear-gradient(135deg, #ffffff 0%, #f8fafc 100%);
            border: 1px solid #e2e8f0;
            border-radius: 12px;
            box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 10px 10px -5px rgba(0, 0, 0, 0.04);
            min-width: 200px;
            max-width: 250px;
            padding: 8px;
            display: none;
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
            font-size: 14px;
            line-height: 1.5;
            backdrop-filter: blur(8px);
            border-top: 3px solid #3b82f6;
        `;
        
        // Add CSS for dropdown items if not already added
        if (!document.querySelector('#custom-dropdown-styles')) {
            const style = document.createElement('style');
            style.id = 'custom-dropdown-styles';
            style.textContent = `
                .custom-dropdown-item {
                    padding: 0;
                    margin: 2px 0;
                    border-radius: 8px;
                    overflow: hidden;
                }
                
                .custom-dropdown-link {
                    display: flex;
                    align-items: center;
                    padding: 12px 16px;
                    text-decoration: none;
                    color: #1f2937;
                    transition: all 0.2s cubic-bezier(0.4, 0, 0.2, 1);
                    border: none;
                    width: 100%;
                    box-sizing: border-box;
                    border-radius: 8px;
                    position: relative;
                    font-weight: 500;
                    letter-spacing: 0.025em;
                }
                
                .custom-dropdown-link:hover {
                    background: linear-gradient(135deg, #3b82f6 0%, #1d4ed8 100%);
                    color: white;
                    text-decoration: none;
                    transform: translateY(-1px);
                    box-shadow: 0 4px 12px rgba(59, 130, 246, 0.3);
                }
                
                .custom-dropdown-link i {
                    margin-right: 12px;
                    width: 18px;
                    text-align: center;
                    font-size: 16px;
                    color: #6366f1;
                    transition: all 0.2s ease;
                }
                
                .custom-dropdown-link:hover i {
                    color: white;
                    transform: scale(1.1);
                }
                
                .custom-dropdown-link span {
                    flex: 1;
                    font-weight: 600;
                }
                
                .custom-dropdown-menu.show {
                    animation: dropdownSlideIn 0.3s cubic-bezier(0.4, 0, 0.2, 1) forwards;
                }
                
                @keyframes dropdownSlideIn {
                    from {
                        opacity: 0;
                        transform: translateY(-10px) scale(0.95);
                    }
                    to {
                        opacity: 1;
                        transform: translateY(0) scale(1);
                    }
                }
                
                .custom-dropdown-item:first-child .custom-dropdown-link {
                    border-top-left-radius: 8px;
                    border-top-right-radius: 8px;
                }
                
                .custom-dropdown-item:last-child .custom-dropdown-link {
                    border-bottom-left-radius: 8px;
                    border-bottom-right-radius: 8px;
                }
            `;
            document.head.appendChild(style);
        }
        
        return customDropdown;
    }
    
    function forceInitializeDropdowns() {
        console.log('🚀 COMPLETE REPLACEMENT: Initializing custom dropdown menus...');
        
        // Kill any existing handlers
        document.removeEventListener('click', handleDocumentClick, true);
        
        const dropdownContainers = document.querySelectorAll('[data-kt-menu="true"]');
        console.log('📋 Found KT menu containers:', dropdownContainers.length);
        
        dropdownContainers.forEach((container, index) => {
            console.log(`🔧 Processing container ${index + 1}:`, container);
            
            const button = container.querySelector('button, .kt-menu-toggle');
            const originalDropdown = container.querySelector('.kt-menu-dropdown');
            
            if (button && originalDropdown) {
                console.log(`✅ Found button and dropdown in container ${index + 1}`);
                
                // Create custom dropdown to replace the KT UI one
                const customDropdown = createCustomDropdown(originalDropdown);
                
                // Hide the original dropdown permanently
                originalDropdown.style.display = 'none !important';
                originalDropdown.style.visibility = 'hidden !important';
                
                // Insert custom dropdown after the container
                container.parentNode.insertBefore(customDropdown, container.nextSibling);
                
                // Remove all existing event listeners from button
                const newButton = button.cloneNode(true);
                button.parentNode.replaceChild(newButton, button);
                
                // Add our custom handler
                newButton.addEventListener('click', function(e) {
                    e.preventDefault();
                    e.stopPropagation();
                    console.log('🖱️ Custom button clicked!');
                    
                    // Toggle behavior: if this dropdown is already open, close it
                    if (currentOpenDropdown === customDropdown) {
                        console.log('🔄 Toggling dropdown closed');
                        closeAllCustomDropdowns();
                    } else {
                        // Close all other dropdowns
                        closeAllCustomDropdowns();
                        
                        // Show this custom dropdown
                        showCustomDropdown(customDropdown, newButton);
                    }
                }, { capture: true, passive: false });
                
                // Add click handlers to dropdown items
                customDropdown.addEventListener('click', function(e) {
                    // If clicking on a link, allow navigation and close dropdown
                    if (e.target.tagName === 'A' || e.target.closest('a')) {
                        console.log('🔗 Dropdown link clicked, closing dropdown');
                        closeAllCustomDropdowns();
                        // Let the link navigate naturally
                        return;
                    }
                    
                    // Stop propagation to prevent document click handler
                    e.stopPropagation();
                });
                
                // Store reference for later cleanup
                container._customDropdown = customDropdown;
                
                console.log(`🎯 Attached click handler to button ${index + 1}`);
            } else {
                console.log(`❌ Missing button or dropdown in container ${index + 1}`);
            }
        });
        
        // Add global click handler
        document.addEventListener('click', handleDocumentClick, true);
        
        initialized = true;
        console.log('✅ COMPLETE REPLACEMENT dropdown initialization complete!');
    }
    
    function showCustomDropdown(customDropdown, button) {
        console.log('🔥 Showing CUSTOM dropdown...');
        
        // Position it correctly - TO THE RIGHT of the button
        const rect = button.getBoundingClientRect();
        const dropdownWidth = 220;
        const dropdownHeight = 120; // Estimated height
        
        // Position to the RIGHT of the button with some spacing
        let left = rect.right + 8;
        let top = rect.top;
        
        // Adjust if dropdown would go off the right edge of viewport
        if (left + dropdownWidth > window.innerWidth - 10) {
            // Position to the LEFT of button if no room on right
            left = rect.left - dropdownWidth - 8;
            console.log('🔄 Positioning to left due to viewport constraints');
        }
        
        // Ensure doesn't go off left edge
        if (left < 10) {
            left = 10;
        }
        
        // Adjust vertical position if needed
        if (top + dropdownHeight > window.innerHeight - 20) {
            top = window.innerHeight - dropdownHeight - 20;
        }
        if (top < 10) {
            top = 10;
        }
        
        // Apply positioning and show
        customDropdown.style.left = left + 'px';
        customDropdown.style.top = top + 'px';
        customDropdown.style.display = 'block';
        customDropdown.classList.add('show');
        
        currentOpenDropdown = customDropdown;
        
        console.log('✅ Custom dropdown positioned to RIGHT at:', { left, top, buttonRight: rect.right });
    }
    
    function closeAllCustomDropdowns() {
        const customDropdowns = document.querySelectorAll('.custom-dropdown-menu.show');
        customDropdowns.forEach(dropdown => {
            dropdown.style.display = 'none';
            dropdown.classList.remove('show');
        });
        currentOpenDropdown = null;
    }

    function handleButtonClick(e, dropdown, container) {
        e.preventDefault();
        e.stopPropagation();
        
        console.log('🖱️ CUSTOM Button clicked!');
        console.log('📍 Dropdown:', dropdown);
        console.log('📍 Container:', container);
        
        // Close all other dropdowns first
        closeAllDropdowns();
        
        // Force show this dropdown
        forceShowDropdown(dropdown, e.target);
    }
    
    function forceShowDropdown(dropdown, button) {
        console.log('💪 FORCE showing dropdown with ULTIMATE power...');
        
        // Position it correctly first
        const rect = button.getBoundingClientRect();
        const dropdownWidth = 200;
        
        let left = rect.right - dropdownWidth;
        let top = rect.bottom + 8;
        
        // Keep within viewport
        if (left < 10) left = 10;
        if (left + dropdownWidth > window.innerWidth - 10) {
            left = window.innerWidth - dropdownWidth - 10;
        }
        if (top + 200 > window.innerHeight - 20) {
            top = rect.top - 200 - 8;
            if (top < 10) top = 10;
        }
        
        // ULTIMATE AGGRESSIVE APPROACH - Set styles multiple times
        const ultimateStyles = {
            display: 'block',
            opacity: '1',
            visibility: 'visible',
            position: 'fixed',
            zIndex: '999999',
            pointerEvents: 'auto',
            transform: 'none',
            background: 'white',
            border: '2px solid #e5e7eb',
            borderRadius: '8px',
            boxShadow: '0 10px 25px rgba(0, 0, 0, 0.15)',
            minWidth: '180px',
            maxWidth: '220px',
            padding: '8px 0',
            left: left + 'px',
            top: top + 'px'
        };
        
        // Apply styles using multiple methods
        Object.keys(ultimateStyles).forEach(key => {
            dropdown.style.setProperty(key.replace(/([A-Z])/g, '-$1').toLowerCase(), ultimateStyles[key], 'important');
        });
        
        // Also use cssText as backup
        dropdown.style.cssText = `
            display: block !important;
            opacity: 1 !important;
            visibility: visible !important;
            position: fixed !important;
            z-index: 999999 !important;
            pointer-events: auto !important;
            transform: none !important;
            background: white !important;
            border: 2px solid #e5e7eb !important;
            border-radius: 8px !important;
            box-shadow: 0 10px 25px rgba(0, 0, 0, 0.15) !important;
            min-width: 180px !important;
            max-width: 220px !important;
            padding: 8px 0 !important;
            left: ${left}px !important;
            top: ${top}px !important;
        `;
        
        // Add classes and attributes
        dropdown.classList.add('show');
        dropdown.classList.remove('hiding', 'hidden');
        dropdown.setAttribute('data-kt-menu-state', 'show');
        dropdown.removeAttribute('hidden');
        
        // Create a mutation observer to fight any attempts to hide it
        const observer = new MutationObserver(function(mutations) {
            mutations.forEach(function(mutation) {
                if (mutation.type === 'attributes' && mutation.attributeName === 'style') {
                    const currentDisplay = dropdown.style.display;
                    if (currentDisplay === 'none' || currentDisplay === '') {
                        console.log('⚠️ Fighting back against style changes!');
                        dropdown.style.cssText = `
                            display: block !important;
                            opacity: 1 !important;
                            visibility: visible !important;
                            position: fixed !important;
                            z-index: 999999 !important;
                            pointer-events: auto !important;
                            transform: none !important;
                            background: white !important;
                            border: 2px solid #e5e7eb !important;
                            border-radius: 8px !important;
                            box-shadow: 0 10px 25px rgba(0, 0, 0, 0.15) !important;
                            min-width: 180px !important;
                            max-width: 220px !important;
                            padding: 8px 0 !important;
                            left: ${left}px !important;
                            top: ${top}px !important;
                        `;
                    }
                }
            });
        });
        
        // Start observing
        observer.observe(dropdown, {
            attributes: true,
            attributeFilter: ['style', 'class'],
            subtree: false
        });
        
        // Store observer for cleanup
        dropdown._observer = observer;
        
        currentOpenDropdown = dropdown;
        
        // Force refresh the styles every 10ms for 1 second to ensure visibility
        let attempts = 0;
        const maxAttempts = 100; // 1 second
        const intervalId = setInterval(() => {
            attempts++;
            if (dropdown.style.display !== 'block') {
                console.log(`🔄 Refreshing dropdown visibility (attempt ${attempts})`);
                dropdown.style.cssText = `
                    display: block !important;
                    opacity: 1 !important;
                    visibility: visible !important;
                    position: fixed !important;
                    z-index: 999999 !important;
                    pointer-events: auto !important;
                    transform: none !important;
                    background: white !important;
                    border: 2px solid #e5e7eb !important;
                    border-radius: 8px !important;
                    box-shadow: 0 10px 25px rgba(0, 0, 0, 0.15) !important;
                    min-width: 180px !important;
                    max-width: 220px !important;
                    padding: 8px 0 !important;
                    left: ${left}px !important;
                    top: ${top}px !important;
                `;
            }
            
            if (attempts >= maxAttempts) {
                clearInterval(intervalId);
                console.log('✅ Dropdown visibility enforcement complete!');
            }
        }, 10);
        
        console.log('🏆 ULTIMATE Dropdown forced visible at position:', { left, top });
    }
    
    function handleDocumentClick(e) {
        // Close custom dropdowns if clicking outside
        if (!e.target.closest('[data-kt-menu="true"], .custom-dropdown-menu')) {
            closeAllCustomDropdowns();
        }
    }
    
    function showDropdown(dropdown) {
        console.log('👀 Showing dropdown...');
        
        // Force show with multiple methods
        dropdown.style.display = 'block !important';
        dropdown.style.visibility = 'visible !important';
        dropdown.style.opacity = '1 !important';
        dropdown.classList.add('show');
        dropdown.classList.remove('hiding');
        
        // Position it
        const menu = dropdown.closest('.kt-menu');
        const button = menu.querySelector('button');
        const rect = button.getBoundingClientRect();
        
        dropdown.style.position = 'fixed';
        dropdown.style.left = (rect.right - 200) + 'px';
        dropdown.style.top = (rect.bottom + 5) + 'px';
        dropdown.style.zIndex = '99999';
        
        currentOpenDropdown = dropdown;
        
        console.log('✅ Dropdown should be visible now!');
    }
    
    function globalClick(e) {
        if (!e.target.closest('.kt-menu, .kt-menu-dropdown')) {
            closeAllDropdowns();
        }
    }

    function openDropdown(dropdown) {
        console.log('Opening dropdown');
        
        // Close any other open dropdown first
        closeAllDropdowns();
        
        // Show dropdown
        dropdown.classList.remove('hiding');
        dropdown.classList.add('show');
        dropdown.style.display = 'block';
        
        // Set current open dropdown
        currentOpenDropdown = dropdown;
        
        // Position dropdown if needed
        positionDropdown(dropdown);
        
        console.log('Dropdown opened successfully');
    }

    function closeDropdown(dropdown) {
        if (!dropdown) return;
        
        console.log('Closing dropdown');
        
        // Stop the mutation observer
        if (dropdown._observer) {
            dropdown._observer.disconnect();
            delete dropdown._observer;
        }
        
        // Add hiding class for animation
        dropdown.classList.add('hiding');
        dropdown.classList.remove('show');
        
        // Hide dropdown after animation
        setTimeout(() => {
            dropdown.style.display = 'none';
            dropdown.classList.remove('hiding');
        }, 300);
        
        // Clear current open dropdown
        if (currentOpenDropdown === dropdown) {
            currentOpenDropdown = null;
        }
    }

    function closeAllDropdowns() {
        const openDropdowns = document.querySelectorAll('.kt-menu-dropdown.show');
        openDropdowns.forEach(dropdown => {
            closeDropdown(dropdown);
        });
        currentOpenDropdown = null;
    }

    function positionDropdown(dropdown) {
        const trigger = dropdown.closest('.kt-menu').querySelector('.kt-menu-toggle');
        const triggerRect = trigger.getBoundingClientRect();
        const viewportWidth = window.innerWidth;
        const viewportHeight = window.innerHeight;
        const dropdownWidth = 220; // Fixed width from CSS
        
        console.log('Positioning dropdown:', {
            triggerRect: triggerRect,
            viewportWidth: viewportWidth,
            dropdownWidth: dropdownWidth
        });
        
        // Calculate initial position (bottom-right of trigger)
        let left = triggerRect.right - dropdownWidth;
        let top = triggerRect.bottom + 8;
        
        // Ensure dropdown doesn't go beyond left edge
        if (left < 10) {
            left = 10;
        }
        
        // Ensure dropdown doesn't go beyond right edge
        if (left + dropdownWidth > viewportWidth - 10) {
            left = viewportWidth - dropdownWidth - 10;
        }
        
        // Ensure dropdown doesn't go beyond bottom edge
        if (top + 200 > viewportHeight - 20) {
            // Position above the trigger
            top = triggerRect.top - 200 - 8;
            if (top < 10) {
                top = 10;
            }
        }
        
        // Apply the calculated position
        dropdown.style.position = 'fixed';
        dropdown.style.left = left + 'px';
        dropdown.style.top = top + 'px';
        dropdown.style.right = 'auto';
        dropdown.style.bottom = 'auto';
        
        console.log('Final dropdown position:', {
            left: left,
            top: top,
            calculatedRight: left + dropdownWidth
        });
    }

    // Initialize with multiple attempts
    function startInitialization() {
        forceInitializeDropdowns();
        setTimeout(forceInitializeDropdowns, 500);
        setTimeout(forceInitializeDropdowns, 1000);
        setTimeout(forceInitializeDropdowns, 2000);
    }
    
    // Export for manual use
    window.initializeEnhancedDropdowns = function() {
        console.log('🔄 Manual FORCE initialization called');
        initialized = false;
        forceInitializeDropdowns();
    };
    
    // Start immediately
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', startInitialization);
    } else {
        startInitialization();
    }

})();
