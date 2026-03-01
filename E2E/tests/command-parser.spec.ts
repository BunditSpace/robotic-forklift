import {
  test,
  expect,
} from '@playwright/test';

test('command parser', async ({
  page,
}) => {
  await page.goto('/command');
  await page
    .getByRole('link', {
      name: 'Movement Command',
    })
    .click();
  await page
    .getByRole('textbox', {
      name: 'Enter Command String',
    })
    .click();
  await page
    .getByRole('textbox', {
      name: 'Enter Command String',
    })
    .fill('F10B5R360');
  await page
    .getByRole('button', {
      name: 'Execute',
    })
    .click();
  await expect(
    page.getByRole(
      'heading',
      {
        name: 'Execution Steps',
      },
    ),
  ).toBeVisible();
  await expect(
    page.getByText(
      '1Move Forward by 10 metres.',
    ),
  ).toBeVisible();
  await expect(
    page.getByText(
      'Move Backward by 5 metres.',
    ),
  ).toBeVisible();
  await expect(
    page.getByText(
      '3Turn Right by 360 degrees.',
    ),
  ).toBeVisible();
});
