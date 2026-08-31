using UrlShortener.Api.Services;
using Xunit;

namespace UrlShortener.Tests;

public class Base62EncoderTests
{
	[Fact]
	public void Encode_Zero_ReturnsFirstAlphabetCharacter()
	{
		var result = Base62Encoder.Encode(0);

		Assert.Equal("a", result);
	}

	[Theory]
	[InlineData(1, "b")]
	[InlineData(61, "9")]
	[InlineData(62, "ba")]
	public void Encode_KnownValues_ReturnsExpectedCode(int input, string expected)
	{
		var result = Base62Encoder.Encode(input);

		Assert.Equal(expected, result);
	}

	[Fact]
	public void Encode_DifferentInputs_ProduceDifferentOutputs()
	{
		var code1 = Base62Encoder.Encode(100);
		var code2 = Base62Encoder.Encode(101);

		Assert.NotEqual(code1, code2);
	}
}