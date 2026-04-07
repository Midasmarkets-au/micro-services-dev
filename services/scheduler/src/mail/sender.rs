use anyhow::Result;
use aws_sdk_sesv2::{
    config::{Credentials, Region},
    primitives::Blob,
    types::{Body, Content, Destination, EmailContent, Message as SesMessage, RawMessage},
    Client,
};
use lettre::{
    message::{Attachment, MultiPart, SinglePart},
    Message,
};

use crate::config::Config;

#[derive(Clone)]
pub struct MailSender {
    client: Client,
    from: String,
}

impl MailSender {
    pub async fn new(config: &Config) -> Result<Self> {
        let creds = Credentials::new(
            &config.ses_access_key,
            &config.ses_secret_key,
            None,
            None,
            "scheduler",
        );
        let sdk_config = aws_config::defaults(aws_config::BehaviorVersion::latest())
            .region(Region::new(config.ses_region.clone()))
            .credentials_provider(creds)
            .load()
            .await;

        Ok(Self {
            client: Client::new(&sdk_config),
            from: config.ses_from.clone(),
        })
    }

    pub async fn send(&self, to: &str, subject: &str, html_body: &str) -> Result<()> {
        self.client
            .send_email()
            .from_email_address(&self.from)
            .destination(Destination::builder().to_addresses(to).build())
            .content(build_simple_content(subject, html_body)?)
            .send()
            .await?;
        Ok(())
    }

    pub async fn send_with_attachment(
        &self,
        to: &[String],
        subject: &str,
        attachment_name: &str,
        attachment_data: Vec<u8>,
    ) -> Result<()> {
        let mut builder = Message::builder().from(self.from.parse()?);
        for addr in to {
            builder = builder.to(addr.parse()?);
        }
        let attachment = Attachment::new(attachment_name.to_string())
            .body(attachment_data, "text/csv; charset=utf-8".parse()?);
        let email = builder
            .subject(subject)
            .multipart(
                MultiPart::mixed()
                    .singlepart(SinglePart::plain(format!(
                        "Please find the {} attached.",
                        subject
                    )))
                    .singlepart(attachment),
            )?;

        self.client
            .send_email()
            .from_email_address(&self.from)
            .content(
                EmailContent::builder()
                    .raw(
                        RawMessage::builder()
                            .data(Blob::new(email.formatted()))
                            .build()?,
                    )
                    .build(),
            )
            .send()
            .await?;
        Ok(())
    }

    pub async fn send_with_cc(
        &self,
        to: &[String],
        cc: &[String],
        subject: &str,
        html_body: &str,
    ) -> Result<()> {
        let mut dest = Destination::builder();
        for addr in to {
            dest = dest.to_addresses(addr);
        }
        for addr in cc {
            dest = dest.cc_addresses(addr);
        }

        self.client
            .send_email()
            .from_email_address(&self.from)
            .destination(dest.build())
            .content(build_simple_content(subject, html_body)?)
            .send()
            .await?;
        Ok(())
    }
}

fn build_simple_content(subject: &str, html_body: &str) -> Result<EmailContent> {
    let content = EmailContent::builder()
        .simple(
            SesMessage::builder()
                .subject(
                    Content::builder()
                        .data(subject)
                        .charset("UTF-8")
                        .build()?,
                )
                .body(
                    Body::builder()
                        .html(
                            Content::builder()
                                .data(html_body)
                                .charset("UTF-8")
                                .build()?,
                        )
                        .build(),
                )
                .build(),
        )
        .build();
    Ok(content)
}
