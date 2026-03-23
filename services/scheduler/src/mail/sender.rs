use anyhow::Result;
use lettre::{
    message::{header::ContentType, Attachment, MultiPart, SinglePart},
    transport::smtp::authentication::Credentials,
    AsyncSmtpTransport, AsyncTransport, Message, Tokio1Executor,
};

use crate::config::Config;

#[derive(Clone)]
pub struct MailSender {
    transport: AsyncSmtpTransport<Tokio1Executor>,
    from: String,
}

impl MailSender {
    pub fn new(config: &Config) -> Result<Self> {
        let creds = Credentials::new(config.smtp_user.clone(), config.smtp_password.clone());

        let transport = AsyncSmtpTransport::<Tokio1Executor>::starttls_relay(&config.smtp_host)?
            .port(config.smtp_port)
            .credentials(creds)
            .build();

        Ok(Self {
            transport,
            from: config.smtp_from.clone(),
        })
    }

    pub async fn send(&self, to: &str, subject: &str, html_body: &str) -> Result<()> {
        let email = Message::builder()
            .from(self.from.parse()?)
            .to(to.parse()?)
            .subject(subject)
            .header(ContentType::TEXT_HTML)
            .body(html_body.to_string())?;

        self.transport.send(email).await?;
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

        self.transport.send(email).await?;
        Ok(())
    }

    pub async fn send_with_cc(
        &self,
        to: &[String],
        cc: &[String],
        subject: &str,
        html_body: &str,
    ) -> Result<()> {
        let mut builder = Message::builder().from(self.from.parse()?);

        for addr in to {
            builder = builder.to(addr.parse()?);
        }
        for addr in cc {
            builder = builder.cc(addr.parse()?);
        }

        let email = builder
            .subject(subject)
            .header(ContentType::TEXT_HTML)
            .body(html_body.to_string())?;

        self.transport.send(email).await?;
        Ok(())
    }
}
