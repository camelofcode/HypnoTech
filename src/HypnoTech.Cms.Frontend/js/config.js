function ConfigViewModel(data, ecommerceEnabled) {
    "use strict";
    var self = this;

    function save(setting) {
        var saveData = {
            config: {
                email: {
                    enableEmails: self.email_enableEmails(),
                    smtpServer: self.email_smtpServer(),
                    mailboxUser: self.email_mailboxUser(),
                    mailboxPassword: self.email_mailboxPassword(),
                    sender: self.email_sender(),
                    defaultRecipient: self.email_defaultRecipient(),
                },
                mainSite: {
                    url: self.mainSite_url(),
                    apiKey: self.mainSite_apiKey(),
                }
            },
            __RequestVerificationToken: document.getElementsByName("__RequestVerificationToken")[0].value
        };

        if (ecommerceEnabled) {
            saveData.config.paypal = {
                apiClientId: self.paypal_apiClientId(),
                apiClientSecret: self.paypal_apiClientSecret(),
                apiUrl: self.paypal_apiUrl(),
                salesUrl: self.paypal_salesUrl(),
                paymentCompleteUrl: self.paypal_paymentCompleteUrl(),
                paymentCancelledUrl: self.paypal_paymentCancelledUrl(),
                isProduction: self.paypal_isProduction()
            };
        }

        ajax.post("/Settings/Config/Save", saveData).done(function () {
            messages.confirmation(setting + " Updated");
        });
    }

    function createTextField(field, displayName, value) {
        self[field] = ko.observable(value);

        self["editing_" + field] = ko.observable(false);


        self["edit_" + field] = function () {
            self["editing_" + field](true);
            self["original_" + field] = self[field]();
        };

        self["cancelEdit_" + field] = function () {
            self[field](self["original_" + field]);
            self["editing_" + field](false);
        };

        self["save_" + field] = function () {
            if (self[field]().trim() === "") {
                
                self[field](self["original_" + field]);
                self["editing_" + field](false);
                return;
            }

            self["editing_" + field](false);
            save(displayName);
        };
    }

    if (ecommerceEnabled) {
        if (data.paypal === null) {
            console.log("No paypal config exists in data store!");
        } else {
            createTextField("paypal_apiClientId", "Paypal Api Client Id", data.paypal.apiClientId);
            createTextField("paypal_apiClientSecret", "Paypal Api Client Secret", data.paypal.apiClientSecret);
            createTextField("paypal_apiUrl", "Paypal Api Url", data.paypal.apiUrl);
            createTextField("paypal_salesUrl", "Paypal Sales Url", data.paypal.salesUrl);
            createTextField("paypal_paymentCompleteUrl", "Paypal Payment Complete Url", data.paypal.paymentCompleteUrl);
            createTextField("paypal_paymentCancelledUrl", "Paypal Payment Cancelled Url", data.paypal.paymentCancelledUrl);

            self.paypal_isProduction = ko.observable(data.paypal.isProduction);
            self.paypal_isProduction.subscribe(function () {
                save("Paypal Mode");
            });
        }
    }

    if (data.email === null) {
        console.log("No email config exists in data store!");
    } else {
        createTextField("email_smtpServer", "Email SMTP Server", data.email.smtpServer);
        createTextField("email_mailboxUser", "Email Mailbox User", data.email.mailboxUser);
        createTextField("email_mailboxPassword", "Email Mailbox Password", data.email.mailboxPassword);
        createTextField("email_sender", "Email Sender", data.email.sender);
        createTextField("email_defaultRecipient", "Email Default Recipient", data.email.defaultRecipient);

        self.email_enableEmails = ko.observable(data.email.enableEmails);
        self.email_enableEmails.subscribe(function () {
            save("Emails Enabled");
        });
    }

    if (data.mainSite === null) {
        console.log("No main site config exists in data store!");
    } else {
        createTextField("mainSite_url", "Main Site Url", data.mainSite.url);
        createTextField("mainSite_apiKey", "Main Site API Key", data.mainSite.apiKey);
    }
}